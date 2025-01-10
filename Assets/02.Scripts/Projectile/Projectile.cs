using UnityEngine;
using Random = UnityEngine.Random;

public class Projectile : MonoBehaviour, IPoolable
{
    public enum RotationMode
    {
        None,             // 회전하지 않음
        AlignToDirection, // 이동 방향에 맞춰 회전
        ConstantSpin      // 일정한 속도로 회전
    }

    [SerializeField] LayerMask targetLayer;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask wallLayer;

    [SerializeField] ProjectileSO data;

    new Rigidbody2D rigidbody;
    new Collider2D collider;

    RotationMode projectileRotationMode;
    float durationTime;
    bool collisionIgnore; // collider 겹침으로 여러 대상을 공격하게 되는 것 방지

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();

        projectileRotationMode = (RotationMode)data.rotationmode;
    }

    public void Initialize(Vector2 startVelocity)
    {
        rigidbody.velocity = startVelocity;
        collisionIgnore = false;

        durationTime = data.duration;
    }

    void Update()
    {
        durationTime -= Time.deltaTime;
        if (durationTime <= 0)
        {
            PoolManager.Instance.ReleaseComponent(gameObject, data.id);
            return;
        }

        switch (projectileRotationMode)
        {
            case RotationMode.AlignToDirection:
                if (rigidbody.velocity != Vector2.zero)
                {
                    float angle = Mathf.Atan2(rigidbody.velocity.y, rigidbody.velocity.x) * Mathf.Rad2Deg; // 방향을 각도로 변환
                    transform.rotation = Quaternion.Euler(0, 0, angle); // 2D 평면에 맞게 회전
                }
                break;
            case RotationMode.ConstantSpin:
                transform.Rotate(0, 0, data.rotationspeed * Time.deltaTime); // Z축 기준으로 회전
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collisionIgnore) return;

        // 벽에 닿아 사라짐
        if (data.isblockedatwall == true && collision.gameObject.IsLayerMatched(wallLayer))
        {
            PoolManager.Instance.ReleaseComponent(gameObject, data.id);
        }
        // 땅에 닿아 사라짐
        else if (data.persistongroundhit == false && collision.gameObject.IsLayerMatched(groundLayer))
        {
            PoolManager.Instance.ReleaseComponent(gameObject, data.id);
        }
        // 대상에 적중
        else if (collision.gameObject.IsLayerMatched(targetLayer))
        {
            if (collision.CompareTag(Tags.T_CanBecomeTarget) == false) return;

            float hitChance = Random.Range(0f, 1f);
            // 명중(by 명중률)
            if (hitChance < data.accuracy)
            {
                IDamageable health = collision.GetComponent<IDamageable>();
                health?.TakeDamage(data.damage, rigidbody.velocity.x < 0);

                // 적중 시 사라짐
                if (data.issplash == false)
                {
                    collisionIgnore = true;
                    PoolManager.Instance.ReleaseComponent(gameObject, data.id);
                }
            }
        }
    }

    public void OnGet()
    {
        // 폭발 사용
        if (data.explosionid != string.Empty)
        {
            // TODO
        }
    }
}