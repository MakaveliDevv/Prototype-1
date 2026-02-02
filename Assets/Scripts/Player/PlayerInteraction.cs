using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerInteraction
    {
        private readonly FragmentInteraction frgInter;
        private readonly DoorInteraction doorInter;
        private readonly Camera camera;

        public DoorInteraction Door => doorInter;

        public PlayerInteraction(PlayerEntity entity)
        {
            camera = entity.camera;

            frgInter = new(entity, camera);
            doorInter = new(entity);
        }

        public void Update()
        {
            frgInter.Update();
            doorInter.Update();
        }
    }
}
