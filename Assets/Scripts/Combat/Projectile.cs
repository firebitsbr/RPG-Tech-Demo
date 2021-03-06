using RPG.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 10f;
        [SerializeField] bool isHoming = false;
        [SerializeField] bool passThroughEnemy = false;
        [SerializeField] bool passThroughPlayer = false;

        [SerializeField] GameObject hitEffect = null;
        [SerializeField] float maxLifeTime = 5f;
        [SerializeField] GameObject[] destroyOnImpact = null;
        [SerializeField] float lifeAfterImpact = 2f;
        [SerializeField] UnityEvent onProjectileLaunch;
        [SerializeField] UnityEvent onProjectileLand;

        Health target = null;
        GameObject instigator = null;
        float damage = 0;

        private void Start()
        {
            transform.LookAt(GetAimLocation());
            onProjectileLaunch.Invoke();
        }

        private void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            if (target == null) { return; }
            if (isHoming && !target.IsDead())
            {
                transform.LookAt(GetAimLocation());
            }
        }

        public void SetTarget(Health target, GameObject instigator, float damage)
        {
            this.target = target;
            this.damage += damage;
            this.instigator = instigator;

            Destroy(gameObject, maxLifeTime);
        }

        private Vector3 GetAimLocation()
        {
            CapsuleCollider targetCapsule = target.GetComponent<CapsuleCollider>();
            if (targetCapsule == null)
            {
                return target.transform.position;
            }
            return target.transform.position + targetCapsule.center;
        }

        private void OnTriggerEnter(Collider other)
        {
            Health otherHealth;
            if (!other.CompareTag("Player") && !other.CompareTag("Enemy"))
            {
                DestroyProjectile();
                return;
            }

            otherHealth = other.GetComponent<Health>();
            if (otherHealth.IsDead()) { return; }

            if (other.CompareTag("Player") && passThroughPlayer) { return; }
            if (other.CompareTag("Enemy") && passThroughEnemy) { return; }

            // if (other != target.GetComponent<Collider>() || other.GetComponent<TerrainCollider>())
            // {
            //     DestroyProjectile();
            //     return;
            // }

            if (otherHealth != target)
            {
                target = otherHealth;
            }
            onProjectileLand.Invoke();
            target.TakeDamage(instigator, damage);

            if (hitEffect != null)
            {
                Instantiate(hitEffect, GetAimLocation(), transform.rotation);
            }

            DestroyProjectile();
        }

        private void DestroyProjectile()
        {
            foreach (GameObject toDestroy in destroyOnImpact)
            {
                speed = 0;
                Destroy(toDestroy);
            }
            Destroy(gameObject, lifeAfterImpact);
        }
    }
}