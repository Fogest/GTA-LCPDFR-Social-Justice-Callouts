using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;
using Rage.Native;
using System.Collections.Generic;
using LSPD_First_Response.Engine;
using System.Linq;
using System.Windows.Forms;
using System;

namespace Social_Justice_Callouts.Callouts
{
    [CalloutInfo("Karen", CalloutProbability.Always)]
    public class Karen : Callout
    {
        private Ped karen;
        private Rage.Object phone;
        private Blip karenBlip;

        private SpawnPoint[] gasStations = {new SpawnPoint(101.544f, new Vector3(1138.212f, -981.2877f, 46.41584f)),
                                    new SpawnPoint(300.275f, new Vector3(1161.893f, -323.5991f, 69.20506f)),
                                    new SpawnPoint(173.7217f, new Vector3(2556.82f, 382.1101f, 108.6229f)),
                                    new SpawnPoint(243.0127f, new Vector3(-48.36202f, -1756.755f, 29.42101f)),
                                    new SpawnPoint(85.09929f, new Vector3(26.65625f, -1346.697f, 29.49702f)),
                                    new SpawnPoint(288.9138f, new Vector3(-708.1245f, -914.675f, 19.21559f)),
                                    new SpawnPoint(229.7006f, new Vector3(-1223.479f, -906.524f, 12.32635f)),
                                    new SpawnPoint(324.0993f, new Vector3(-1488.007f, -380.102f, 40.16343f)),
                                    new SpawnPoint(68.896f, new Vector3(374.2217f, 326.6744f, 103.5664f)),
                                    new SpawnPoint(326.2695f, new Vector3(-1821.324f, 792.9097f, 138.1233f)),
                                    new SpawnPoint(275.5166f, new Vector3(-2968.85f, 390.9151f, 15.04332f)),
                                    new SpawnPoint(186.2491f, new Vector3(-3040.119f, 586.3646f, 7.908929f)),
                                    new SpawnPoint(0.03357552f, new Vector3(1166.148f, 2708.433f, 38.15771f)),
                                    new SpawnPoint(262.3131f, new Vector3(547.3481f, 2670.397f, 42.15649f)),
                                    new SpawnPoint(109.0365f, new Vector3(1961.688f, 3741.66f, 32.34374f)),
                                    new SpawnPoint(138.9744f, new Vector3(2678.369f, 3281.494f, 55.24115f)),
                                    new SpawnPoint(157.638f, new Vector3(1699.094f, 4924.68f, 42.06367f)),
                                    new SpawnPoint(51.47797f, new Vector3(1729.435f, 6414.739f, 35.03722f)),
                                    };
        private Vector3 spawnPoint;

        private readonly List<string> dialogWithPed = new List<string>()
                {
                    "~r~Customer~w~: THIS IDIOT IS TRYING TO RIP ME OFF!",
                    "~b~Officer~w~: Okay, I am going to need to calm down and explain what happened.",
                    "~r~Customer~w~: ALL YOU NEED TO KNOW IS THIS FUCKER NEEDS TO GIVE MY MONEY BACK",
                    "~o~Attendant~w~: Officer, they have been yelling at me for the last 10 minutes and refuses to leave the store.",
                    "~o~Attendant~w~: They bought and ate two hotdogs and are now asking for their money back for them.",
                    "~r~Customer~w~: AND I DON'T LIKE THEM AND WANT MY MONEY BACK NOW!!!",
                    "~o~Attendant~w~: I told em they ate both of them and they aren't getting their money back and needs to leave",
                    "~b~Officer~w~: You have been asked to leave the store, you need to leave the store now.",
                };
        private int dialogWithPedIndex = 0;
        private enum Stages
        {
            Beginning,
            Dialogue,
            PostDialogue,
            Action,
            End
        }
        private Stages calloutStage = Stages.Beginning;
        private bool hasPedLooked = false;


        public override bool OnBeforeCalloutDisplayed()
        {
            spawnPoint = gasStations.OrderBy(x => x.Position.DistanceTo(Game.LocalPlayer.Character.Position)).FirstOrDefault();

            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 15f);
            AddMinimumDistanceCheck(5f, spawnPoint);

            CalloutMessage = "Irriate Customer";
            CalloutPosition = spawnPoint;

            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_DISTURBING_THE_PEACE_01", spawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            karen = new Ped(spawnPoint);

            if (!karen.Exists()) return false;

            karenBlip = karen.AttachBlip();

            phone = new Rage.Object("prop_npc_phone", Vector3.Zero);

            if (phone.Exists()) 
                phone.AttachTo(karen, karen.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.1490f, 0.0560f, -0.0100f), new Rotator(-17f, -142f, -151f));
            karen.Tasks.PlayAnimation("cellphone@", "cellphone_photo_idle", 1.3f, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);

            karen.BlockPermanentEvents = true;

            //Ped[] nearbyPeds = karen.GetNearbyPeds(5);
            //Ped storeClerk = nearbyPeds.OrderBy(x => x.Position.DistanceTo(karen.Position)).FirstOrDefault();
            //karen.Tasks.AchieveHeading(MathHelper.ConvertDirectionToHeading(storeClerk.Direction - karen.Direction));


            Game.DisplaySubtitle("A gas station ~o~attendant~w~ reports a ~r~customer~w~ yelling in the store and refusing to leave", 10000);
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (this.calloutStage == Stages.Beginning)
            {
                Game.DisplayHelp("Press ~b~ Y~s~ to speak to customer when in the store");
                this.calloutStage = Stages.Dialogue;
            }

            else if (this.calloutStage == Stages.Dialogue)
            {
                this.HaveConversation();

                if (!this.hasPedLooked && karen.DistanceTo(Game.LocalPlayer.Character) <= 3f)
                {
                    Game.DisplayHelp("Player is in range to be looked at");
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(karen, Game.LocalPlayer.Character, -1);
                    this.hasPedLooked = true;
                }
            }
            else if (this.calloutStage == Stages.PostDialogue)
            {
                // This will decide which alternative ending to run and then run it on a new fiber.
                Random r = new Random();
                int chance = r.Next(0, 100);

                Game.DisplayNotification("Starting SituationLeaveAggressively()");

                //SituationLeavePeacefully();    -- CHECKED, seems to work quite well, should be good to go!
                //SituationLeaveAggressively();  -- CHECKED, works, just needs a bit of cleaning up.
                //SituationLeaveViolently();     -- CHECKED, works
                //SituationLeaveViolently(true); -- CHECKED, works, though people tazed seem to still sit up and keep shooting back

                if (chance < 30)
                {
                    SituationLeavePeacefully();
                }
                else if (chance < 60)
                {
                    SituationLeaveAggressively();
                }
                else if (chance < 85)
                {
                    SituationLeaveViolently();
                }
                else
                {
                    SituationLeaveViolently(true);
                }

                this.calloutStage = Stages.Action;
            }

            if (karen.IsDead || Functions.IsPedArrested(karen) || this.calloutStage == Stages.End)
            {
                End();
            }
            if (karen.IsRagdoll)
            {
                Functions.SetPedCantBeArrestedByPlayer(karen, false);
            }
        }

        private void HaveConversation()
        {
            if (!Game.IsKeyDown(Keys.Y)) return;
            else if (karen.DistanceTo(Game.LocalPlayer.Character) > 5f) return;

            if (dialogWithPedIndex < dialogWithPed.Count)
            {
                Game.DisplaySubtitle(dialogWithPed[dialogWithPedIndex], 20000);
                dialogWithPedIndex++;
            }
            else
            {
                Game.DisplayNotification("Done dialogue");
                this.calloutStage = Stages.PostDialogue;
            }
        }

        private void SituationLeavePeacefully()
        {
            GameFiber.StartNew(delegate
            {
                try
                {
                    Game.DisplaySubtitle("~r~Customer~w~: Okay, okay, okay! I am leaving now dude, don't have to be rude!", 7000);
                    karen.Tasks.PlayAnimation("cellphone@", "cellphone_photo_exit", 2f, AnimationFlags.UpperBodyOnly);
                    GameFiber.Wait(2000);
                    karen.Tasks.Clear();
                    phone.Delete();

                    karen.Dismiss(); //We will see if just dismissing them is enough to have them leave.
                    GameFiber.Wait(5000);
                    this.calloutStage = Stages.End;
                }
                catch (System.Threading.ThreadAbortException e)
                {
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Social Justice Callouts handled the exception successfully.");
                    Game.DisplayNotification("~O~Karen~s~ callout crashed, sorry. Please send me your log file <3");
                    Game.DisplayNotification("Full LSPDFR crash prevented ~g~successfully.");
                    End();
                }
                catch (Exception e)
                {
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Social Justice Callouts handled the exception successfully.");
                    Game.DisplayNotification("~O~Karen~s~ callout crashed, sorry. Please send me your log file <3");
                    Game.DisplayNotification("Full LSPDFR crash prevented ~g~successfully.");
                    End();
                }
            });
        }

        private void SituationLeaveAggressively()
        {
            GameFiber.StartNew(delegate
            {
                try
                {
                    Game.DisplayHelp("No key interaction needed for this conversation");
                    Game.DisplaySubtitle("~r~Customer~w~: Why, what are you gonna do? Shoot me?!", 5000);

                    GameFiber.Wait(3500);
                    Game.DisplaySubtitle("~b~Officer~w~: You need to leave the store or I will forcibly remove you.", 5000);
                    GameFiber.Wait(750);

                    karen.Tasks.PlayAnimation("cellphone@", "cellphone_photo_exit", 2f, AnimationFlags.UpperBodyOnly);
                    GameFiber.Wait(2000);

                    karen.Tasks.Clear();
                    phone.Delete();

                    GameFiber.Wait(500);
                    karen.Tasks.PlayAnimation("random@domestic", "balcony_fight_idle_female", 1.2f, AnimationFlags.Loop);

                    Game.DisplaySubtitle("~b~Officer~w~: If you continue acting hostile you are going to get tazed", 5000);
                    GameFiber.Wait(5000);

                    Game.DisplaySubtitle("~r~Customer~w~: FUCK YOU!", 5000);
                    karen.Tasks.Clear();
                    GameFiber.Wait(300);
                    karen.Tasks.Flee(Game.LocalPlayer.Character, 150f, 20000);
                    GameFiber.Wait(5000);
                    this.calloutStage = Stages.End;
                }
                catch (System.Threading.ThreadAbortException e)
                {
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Social Justice Callouts handled the exception successfully.");
                    Game.DisplayNotification("~O~Karen~s~ callout crashed, sorry. Please send me your log file <3");
                    Game.DisplayNotification("Full LSPDFR crash prevented ~g~successfully.");
                    End();
                }
                catch (Exception e)
                {
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Social Justice Callouts handled the exception successfully.");
                    Game.DisplayNotification("~O~Karen~s~ callout crashed, sorry. Please send me your log file <3");
                    Game.DisplayNotification("Full LSPDFR crash prevented ~g~successfully.");
                    End();
                }
            });
        }

        private void SituationLeaveViolently(bool armed = false)
        {
            GameFiber.StartNew(delegate
            {
                try
                {
                    Game.DisplaySubtitle("~r~Customer~w~: Fuck you motherfucker, I pay your taxes!!!", 5000);
                    karen.Tasks.PlayAnimation("cellphone@", "cellphone_photo_exit", 2f, AnimationFlags.UpperBodyOnly);
                    GameFiber.Wait(2000);

                    karen.Tasks.Clear();
                    phone.Delete();

                    GameFiber.Wait(500);
                    karen.Tasks.PlayAnimation("random@domestic", "balcony_fight_idle_female", 1.2f, AnimationFlags.Loop);

                    Game.DisplaySubtitle("~b~Officer~w~: If you continue acting hostile you are going to get tazed", 5000);
                    GameFiber.Wait(5000);

                    karen.Tasks.Clear();
                    Game.DisplaySubtitle("~r~Customer~w~: I am gonna fuck you up asshole", 5000);
                    if (armed)
                    {
                        Random r = new Random();
                        int chance = r.Next(0, 100);

                        if (chance < 33)
                        {
                            karen.Inventory.GiveNewWeapon("WEAPON_PISTOL", -1, true);
                        }
                        else if (chance < 66)
                        {
                            karen.Inventory.GiveNewWeapon("WEAPON_KNIFE", -1, true);
                        } 
                        else
                        {
                            karen.Inventory.GiveNewWeapon("WEAPON_STUNGUN", -1, true);
                        }
                    }
                    GameFiber.Wait(1750);
                    Functions.SetPedCantBeArrestedByPlayer(karen, false);
                    karen.Tasks.FightAgainst(Game.LocalPlayer.Character);
                }
                catch (System.Threading.ThreadAbortException e)
                {
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Social Justice Callouts handled the exception successfully.");
                    Game.DisplayNotification("~O~Karen~s~ callout crashed, sorry. Please send me your log file <3");
                    Game.DisplayNotification("Full LSPDFR crash prevented ~g~successfully.");
                    End();
                }
                catch (Exception e)
                {
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Social Justice Callouts handled the exception successfully.");
                    Game.DisplayNotification("~O~Karen~s~ callout crashed, sorry. Please send me your log file <3");
                    Game.DisplayNotification("Full LSPDFR crash prevented ~g~successfully.");
                    End();
                }
            });
        }

        public override void End()
        {
            if (karen.Exists())
            {
                if (karen.IsDead)
                    karen.Delete();
                else if (!Functions.IsPedArrested(karen))
                    karen.Dismiss();
            }
            if (phone.Exists()) phone.Delete();

            if (karenBlip.Exists()) karenBlip.Delete();
            base.End();
        }
    }
}
