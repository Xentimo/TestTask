namespace SourceScipt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    [CreateAssetMenu]
    public class EnemyConfig : ScriptableObject
    {
        public float spawnInterval = 2f;
        public int maxHealth = 100;
        public float speed = 3f;
    }

    public static class GameEvents
    {
        public static event Action PlayerDied;
        public static void RaisePlayerDied() => PlayerDied();
    }

    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance;
        public EnemyConfig config;
        public Enemy enemyPrefab;
        public Button[] weaponButtons;
        public List<Enemy> enemies = new List<Enemy>();
        private float _timer;
        private int _score;
        private int _selectedWeapon;

        void Awake()
        {
            Instance = this;
            for (int i = 0; i < weaponButtons.Length; i++)
                weaponButtons[i].onClick.AddListener(() => _selectedWeapon = i);
        }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer > config.spawnInterval)
            {
                _timer = 0;
                Spawn();
            }

            foreach (var enemy in enemies)
            {
                if (enemy.Health <= 0)
                {
                    enemies.Remove(enemy);
                    Destroy(enemy.gameObject);
                    _score++;
                    GameObject.Find("ScoreLabel").GetComponent<Text>().text =
                        "Score: " + _score;
                }
            }

            var player = GameObject.FindWithTag("Player");
            var closest = enemies
                .OrderBy(e => Vector3.Distance(e.transform.position,
                    player.transform.position))
                .FirstOrDefault();
            if (closest != null)
                closest.Highlight();
        }

        void Spawn()
        {
            var enemy = Instantiate(enemyPrefab, transform.position,
                Quaternion.identity);
            enemy.Init(config);
            enemies.Add(enemy);
            config.spawnInterval *= 0.95f;
        }
    }

    public class Enemy : MonoBehaviour
    {
        public int Health;
        private EnemyConfig _config;
        private Transform _target;

        public void Init(EnemyConfig config)
        {
            _config = config;
            Health = config.maxHealth;
        }

        void OnEnable()
        {
            GameEvents.PlayerDied += OnPlayerDied;
        }

        async void Start()
        {
            _target = GameObject.FindWithTag("Player").transform;
            await Task.Delay(5000);
            GetComponent<Renderer>().material.color = Color.red;
            _config.speed *= 1.5f;
        }

        void Update()
        {
            transform.position = Vector3.MoveTowards(
                transform.position, _target?.position ?? transform.position,
                _config.speed);
        }

        void OnPlayerDied()
        {
            enabled = false;
        }

        public void TakeDamage(int amount)
        {
            Health -= amount;
        }

        public void Highlight()
        {
            GetComponent<Renderer>().material.color = Color.yellow;
        }
    }
}