using UnityEngine;

[CreateAssetMenu(
    fileName = "Weapon_New",
    menuName = "RPG Data/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("기본 정보")]
    public string weaponId;
    public string weaponName;
    public WeaponType weaponType;
    public ClassType classType;
    public bool canStack;
    public Sprite icon;

    [Header("전투 수치")]
    public int damage;
    public float attackRange;
    public float attackRate;
    public float criticalChance;
    public float criticalMultiplier = 1.5f;

    [Header("원거리 무기")]
    public int magazineSize;
    public float reloadTime;
    public bool isRange;
    public bool useAmmo;

    [Header("프리팹 및 이펙트")]
    public GameObject weaponPrefab;
    public GameObject projectilePrefab;
    public ParticleSystem fireEffect;
    public AudioClip attackSound;
}
