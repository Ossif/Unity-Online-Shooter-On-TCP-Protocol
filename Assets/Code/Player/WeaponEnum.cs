using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponEnumIds
{
    public enum WeaponId: uint
    {
        NONE = 0,
        FISTS = 1,
        PISTOL = 2,
        AK = 3,
        SAWNED_OFF = 4,
        GRENADE_LAUNCHER = 5,
        RIFLE = 6
    }

    public class Weapon
    {
        public WeaponId weaponId;
        public GameObject weaponObject;
        public string name;
        public int ammoCartridge;
        public int ammoMax;
        public float damage;
        public bool isAuto;
        public float shotTime;

        public string takeAnim;
        public string shotAnim;
        public string reloadAnim;
        public string walkAnim;

        public Weapon(WeaponId weaponId, GameObject weaponObject, string name, int ammoCartridge, int ammoMax, float damage, bool isAuto, float shotTime, string takeAnim, string shotAnim, string reloadAnim, string walkAnim)
        { 
            this.weaponId = weaponId;
            this.weaponObject = weaponObject;
            this.name = name;
            this.ammoCartridge = ammoCartridge;
            this.ammoMax = ammoMax;
            this.damage = damage;
            this.isAuto = isAuto;
            this.shotTime = shotTime;

            this.takeAnim = takeAnim;
            this.shotAnim = shotAnim;
            this.reloadAnim = reloadAnim;
            this.walkAnim = walkAnim;
        }
    }

    public class WeaponEnum : MonoBehaviour
    {
        public GameObject PistolObject;
        public GameObject AKObject;
        public GameObject SawnedOffObject;

        public List<Weapon> weaponList = new List<Weapon>();

        public void InitializeAllWeapon() 
        {
            weaponList.Add(new Weapon(WeaponId.NONE, null, "none", 0, 0, 0, false, 0, "", "", "", ""));
            weaponList.Add(new Weapon(WeaponId.FISTS, null, "fists", 0, 0, 10.0f, false, 0.3f, "", "", "", ""));
            weaponList.Add(new Weapon(WeaponId.PISTOL, PistolObject, "pistol", 10, 70, 25.0f, false, 0, "pistol_take", "pistol_shot", "pistol_reload", "pistol_walk"));
            weaponList.Add(new Weapon(WeaponId.AK, AKObject, "AK", 30, 120, 20.0f, true, 0.1f, "AK_take", "AK_shot", "AK_reload", "AK_walk"));
            weaponList.Add(new Weapon(WeaponId.SAWNED_OFF, SawnedOffObject, "Sawned-Off", 2, 30, 40.0f, false, 0.5f, "SO_take", "SO_shot", "SO_reload", "SO_walk"));
        }
    }
}
