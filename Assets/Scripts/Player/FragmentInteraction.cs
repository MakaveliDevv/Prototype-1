using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class FragmentInteraction
    {
        private readonly PlayerEntity entity;
        private MemoryFragmentTrigger activeInteraction;
        private MemoryFragmentTrigger currentInteraction;

        private RaycastHit hit;
        private IInteractable interactable;
        private Coroutine interactionRoutine;

        private readonly Camera camera;

        // Button-down detection
        private bool prevInteractPressed;
        private bool prevCancelInteractPressed;
        private bool prevCollectPressed;

        public FragmentInteraction(PlayerEntity entity, Camera camera)
        {
            this.entity = entity;
            this.camera = camera;
        }

        public void Update()
        {
            if(GameManager.Instance.RayCastHit(ref hit, camera.gameObject, entity.p_settings.raycastDistance, entity.p_settings.interactableMask))
            {
                if(hit.collider.TryGetComponent<IInteractable>(out var i))
                    interactable = i;
                else
                    interactable = null;
            }

            if (currentInteraction != null)
            {
                if (InteractCancelDown())
                    EndInteraction();

                return;
            }

            UpdatePromptAndEligibility();

            if (activeInteraction != null && InteractDown() && interactionRoutine == null)
            {
                activeInteraction.OnInteract();
                
                if (activeInteraction.Interacting)
                    interactionRoutine = entity.StartCoroutine(InteractionSequence(activeInteraction));    
            } 
        }

        private void UpdatePromptAndEligibility()
        {
            if (interactable is MemoryFragmentTrigger fragment && hit.collider != null)
            {
                Vector3 a = entity.p_settings.playerBody.position;
                Vector3 b = hit.collider.transform.position;
                
                float dist = GameManager.Instance.CalculatedDistance(a, b);

                if (dist <= entity.p_settings.distanceThreshold && !fragment.Interacting)
                {
                    // Transfer prompt ownership
                    if (activeInteraction != fragment)
                    {
                        HidePrompt();
                        activeInteraction = fragment;
                        fragment.Interact();
                    }

                    return;
                }
            }

            HidePrompt();
        }

        private void HidePrompt()
        {
            if (activeInteraction == null)
                return;

            if (!activeInteraction.Interacting)
                activeInteraction.InteractUI(false);

            activeInteraction = null;
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

        private bool CollectDown()
        {
            bool pressed = InputManager.CollectPressed;
            bool down = pressed && !prevCollectPressed;
            prevCollectPressed = pressed;
            return down;
        }

        // ----------------- Interaction Sequence -----------------

        private IEnumerator InteractionSequence(MemoryFragmentTrigger fragment)
        {
            currentInteraction = fragment;
            entity.SetInputLocked(true);

            // Hide prompt and mark interacting
            fragment.InteractUI(false);
            fragment.IsInteracting(true);

            // Make sure controller is enabled while auto-move
            var controller = entity.charController;
            if (controller != null) controller.enabled = true;

            // 1) Move player to stand position in front of fragment
            Vector3 standPos = ComputeStandPosition(fragment, entity.p_settings.StandOffDistance);
            yield return MoveTo(controller, standPos, entity.p_settings.AutoMoveSpeed);

            // 2) Rotate body to face fragment center
            yield return RotateBodyTo(entity.p_settings.playerBody, FocusPoint(fragment), entity.p_settings.BodyTurnSpeed);

            // 3) Rotate camera to look at fragment center
            yield return FocusCameraTo(camera.transform, FocusPoint(fragment), entity.p_settings.CameraTurnSpeed, entity.p_settings.CameraMinFocusTime);

            // 4) Freeze movement
            if (controller != null) controller.enabled = false;

            // Mark focused
            fragment.isFocused = true;

            // Stay in interaction mode until cancel or end
            while (fragment.Interacting)
            {
                if(CollectDown())
                {
                    fragment.Collect();    
                    fragment.PlayAudio(AudioKey.OnCollect);

                    yield return new WaitForSeconds(2f);

                    // Cleanup
                    fragment.isFocused = false;
                    if (controller != null) controller.enabled = true;
                    entity.SetInputLocked(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    currentInteraction = null;
                    interactionRoutine = null;

                    yield break;
                }
                
                yield return null;
            }

            // Cleanup
            fragment.isFocused = false;
            if (controller != null) controller.enabled = true;
            entity.SetInputLocked(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            currentInteraction = null;
            interactionRoutine = null;

            yield break;
        }

        private Vector3 FocusPoint(MemoryFragmentTrigger fragment)
        {
            var col = fragment.GetComponent<Collider>();
            return col != null ? col.bounds.center : fragment.transform.position;
        }

        private Vector3 ComputeStandPosition(MemoryFragmentTrigger fragment, float distance)
        {
            Vector3 focus = FocusPoint(fragment);
            Vector3 stand = focus - fragment.transform.forward * distance;

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

        public void EndInteraction()
        {
            if (currentInteraction == null)
                return;

            currentInteraction.IsInteracting(false);
        }
    }
}