namespace Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class IManager : MonoBehaviour
    {
        [HideInInspector]
        public bool initOnAwake = false;

        public virtual void Awake() {
            if (initOnAwake) {
                StandardInit();
            }
        }


        public virtual void PreInit() {

        }

        public virtual void Init() {

        }

        public virtual void PostInit() {

        }

        public void StandardInit() {
            PreInit();
            Init();
            PostInit();
        }

        public virtual void DuplicateManagerError() {
            print("[" + this.name + "] Instance is already set, deleting duplicate");
            Destroy(this);
        }

    }

}