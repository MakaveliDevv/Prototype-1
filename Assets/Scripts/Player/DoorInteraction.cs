using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    public class DoorInteraction
    {
        private readonly PlayerEntity entity;

        private LockedDoor candidateDoor; // set by triggers
        private LockedDoor activeDoor;    // currently owning the prompt
        private Coroutine interactionRoutine;

        private bool inRangeForDoor;
        private bool prevInteractPressed;
        private bool prevCancelInteractPressed;

        public DoorInteraction(PlayerEntity entity)
        {
            this.entity = entity;
        }

        private float distanceThreshold;
        public void Update()
        {
            if (!inRangeForDoor || candidateDoor == null)
            {
                HidePrompt();
                return;
            }

            Vector3 a = entity.p_settings.playerBody.position;
            Vector3 b = candidateDoor.transform.position;

            float dist = GameManager.Instance.CalculatedDistance(a, b); 

            if (dist <= distanceThreshold)
            {
                // Transfer prompt ownership
                if (activeDoor != candidateDoor)
                {
                    HidePrompt();
                    activeDoor = candidateDoor;
                    activeDoor.Interact();
                }

                if (activeDoor != null && activeDoor.Interacting)
                {
                    if (InteractCancelDown())
                        EndInteraction();

                    return;
                }

                // Button-down => interact
                if (activeDoor != null && InteractDown() && interactionRoutine == null)
                {
                    activeDoor.OnInteract();

                    if(activeDoor.Interacting)
                        interactionRoutine = entity.StartCoroutine(InteractionSequence(activeDoor));                        
                }

                return;
            }

            // Still in trigger, but not close enough
            HidePrompt();
        }

        // Called by LockedDoor trigger enter
        public void EnterRange(LockedDoor door)
        {
            if (door == null) return;

            inRangeForDoor = true;
            candidateDoor = door;
        }

        // Called by LockedDoor trigger exit
        public void ExitRange(LockedDoor door)
        {
            if (door == null) return;

            if (candidateDoor == door) candidateDoor = null;
            if (activeDoor == door) HidePrompt();

            inRangeForDoor = candidateDoor != null;
        }

        private void HidePrompt()
        {
            if (activeDoor == null) return;

            activeDoor.InteractUI(false);
            activeDoor = null;
        }

        private bool InteractDown()
        {
            bool pressed = InputManager.InteractPressed;
            bool down = pressed && !prevInteractPressed;
            prevInteractPressed = pressed;
            return down;
        }

        private bool InteractCancelDown()
        {
            bool pressed = InputManager.CancelInteractPressed;
            bool down = pressed && !prevCancelInteractPressed;
            prevCancelInteractPressed = pressed;
            return down;
        }

        public void EndInteraction()
        {
            if (activeDoor == null)
                return;

            activeDoor.IsInteracting(false);
        }

        private IEnumerator InteractionSequence(LockedDoor lockedDoor)
        {
            if(lockedDoor.canOpen)
                yield break;
            
            activeDoor = lockedDoor;
            entity.SetInputLocked(true);

            // Hide prompt and mark interacting
            lockedDoor.InteractUI(false);
            lockedDoor.IsInteracting(true);

            // Make sure controller is enabled while auto-move
            var controller = entity.charController;
            if (controller != null) controller.enabled = true;

            // 1) Move player to stand position in front of fragment
            Vector3 standPos = ComputeStandPosition(lockedDoor, entity.p_settings.StandOffDistance);
            yield return MoveTo(controller, standPos, entity.p_settings.AutoMoveSpeed);

            // 2) Rotate body to face fragment center
            yield return RotateBodyTo(entity.p_settings.playerBody, FocusPoint(lockedDoor), entity.p_settings.BodyTurnSpeed);

            // 3) Rotate camera to look at fragment center
            // yield return FocusCameraTo(camera.transform, FocusPoint(lockedDoor), entity.p_settings.CameraTurnSpeed, entity.p_settings.CameraMinFocusTime);

            // 4) Freeze movement
            if (controller != null) controller.enabled = false;

            // Mark focused
            lockedDoor.isFocused = true;

            // Stay in interaction mode until cancel or end
            while (lockedDoor.Interacting)
            {
                // UI logic
                if(!entity.canvas.activeInHierarchy) entity.canvas.SetActive(true);

                lockedDoor.OnInteractUI(true);        

                // Exit the loop if all the fragments has been collected
                if(GameManager.Instance.HasAllFragments(activeDoor.requiredFragments))
                {
                    // Cleanup
                    Cleanup(lockedDoor, controller);
                    activeDoor = null;
                    candidateDoor = null;
                    yield break;    
                }
                
                yield return null;
            }

            // Cleanup
            Cleanup(lockedDoor, controller);
            yield break;
        }

        private void Cleanup(LockedDoor lockedDoor, CharacterController controller)
        {
            // Cleanup
            lockedDoor.IsInteracting(false);
            lockedDoor.isFocused = false;
            if (controller != null) controller.enabled = true;
            entity.SetInputLocked(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            entity.canvas.SetActive(false);

            interactionRoutine = null;
        }

        private Vector3 FocusPoint(LockedDoor lockedDoor)
        {
            var col = lockedDoor.GetComponent<Collider>();
            return col != null ? col.bounds.center : lockedDoor.transform.position;
        }

        private Vector3 ComputeStandPosition(LockedDoor lockedDoor, float distance)
        {
            Vector3 focus = FocusPoint(lockedDoor);
            Vector3 stand = focus - lockedDoor.transform.forward * distance;

            stand.y = entity.p_settings.playerBody.position.y;
            return stand;
        }

        private IEnumerator MoveTo(CharacterController controller, Vector3 target, float speed)
        {
            if (controller == null)
                yield break;

            while (true)
            {
                Vector3 pos = controller.transform.position;
                Vector3 delta = target - pos;
                delta.y = 0f;

                if (delta.sqrMagnitude < 0.01f)
                    yield break;

                Vector3 step = delta.normalized * speed * Time.deltaTime;
                controller.Move(step);

                yield return null;
            }
        }

        private IEnumerator RotateBodyTo(Transform body, Vector3 lookTarget, float degreesPerSecond)
        {
            Vector3 dir = lookTarget - body.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f)
                yield break;

            Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

            while (Quaternion.Angle(body.rotation, targetRot) > 0.5f)
            {
                body.rotation = Quaternion.RotateTowards(body.rotation, targetRot, degreesPerSecond * Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator FocusCameraTo(Transform cam, Vector3 lookTarget, float lerpSpeed, float minTime)
        {
            float t = 0f;

            while (t < minTime)
            {
                Quaternion targetRot = Quaternion.LookRotation((lookTarget - cam.position).normalized);
                cam.rotation = Quaternion.Slerp(cam.rotation, targetRot, Time.deltaTime * lerpSpeed);
                t += Time.deltaTime;
                yield return null;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
