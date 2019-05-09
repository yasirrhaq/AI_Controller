using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_CONTROLLER : MonoBehaviour {

    //Kelas AI Controller adalah tempat anda memberikan kecerdasan buatan pada agen TANK anda
    //Peraturan yang harus dipenuhi adalah sbb :

    // Dilarang mengakses atau mengedit game objek tank enemy secara langsung menggunakan code unity apapun seperti :
    // GameObject.Find ,  GameObject.FindGameObjectsWithTag dan semacamnya, pelanggaran adalah diskualifikasi, nilai = 0, harap tanyakan terlebh dahulu terkait penggunaan fungsi2 tertentu;
    // seluruh script dilarang diedit, hanya script ini (AI_CONTROLLER) saja yang wajib anda ubah, karena script ini saja yg anda kumpulkan, error berarti gagal


    //Mengakses informasi enemmy telah di sediakan di script Tank.cs sbb
    //public Vector3 _infEnemyLastdirection;         //The last direction enemy facing, update when AI sensor found enemy.
    //public Vector3 _infEnemyLastPos;           //The last position of enemy, update when sensor found enemy
    //public int _infEnemyHealth;               //the last health of enemy.  update each frame
    //public int _infisEnemySeen;               //is enemy seen in sensor now.

    //contoh cara akses informasi di atas melalui kelas AI_CONTROLLER ini :
    //Vector3 a = GetComponent<Tank>()._infEnemyLastPos;
    //seluruh atribut kelas Tank.cs yang berawalan "_inf.." contoh _infEnemyHealth, boleh diakses
    //info mengenai atribut2 yang berawalan "_inf.." tsb silahkan buka pada kelas Tank.cs

    //Sensor OBSTACLE WALL dan MUSUH
    // tank anda telah dibekali sensor otomatis untuk mendeteksi wall dan musuh
    // sensor wall dengan  radius lingkaran 6 unit, sensor musuh segitiga sama kaki di depan tank 1.5 unit, Tinggi 12 unit dan alas = 10 unit
    // informasi dari sensor wall adalah posisi center tiap wall yg terdeteksi sensor, wall memiliki sisi selebar 1 unit bentuk persegi
    // informasi dari sensor wall dapat diakses di "GetComponent<Tank>()._infLastposWall" , berupa list<vector3> posisi center wall tersebut
    // informasi dari sensor musuh dapat diakses di "_infEnemyLastdirection, _infEnemyLastPos, _infEnemyHealth" diakses dengan getcomponent juga 

    // note untuk testing silahkan sesuaikan nilai attribut id pada kelas Tank.cs, id = 0 untuk player 1 dan id = 1 untuk player 2
    // silahkan konsulkan dengan dosen apa yg telah anda kerjakan agar tdk terkena diskualifikasi
    // silahkan melakukan read pada objek transform atau rigidbody tank anda sendiri namun tidak diperkenankan mengubah nilainya secara langsung
    // harus melalui methode yg di sediakan di kelas Tank.cs : "Move, Turn, Shoot"

    //MENGGERAKAN TANK, TANK MOVEMENT
    // Gerak MAJU dan MUNDUR
    //GetComponent<Tank>().Move(1); untuk maju
    //GetComponent<Tank>().Move(-1) ; untuk mundur

    // Gerak MEMUTAR atau merotasi arah depan tank
    //GetComponent<Tank>().Turn(-1); untuk putar ke kiri berlawanan jarum jam
    //GetComponent<Tank>().Turn(1); untuk putar ke kanan searah jarum jam

    // Menembak //SHOOTING
    //GetComponent<Tank>().Shoot()
    //peluru hanya 1, dan reload setiap 2 detik secara otomatis

    //telah disediakan framework behavior tree jika ingin menggunakan, silahkan bebas menggunakan rumus apapun, menggunakan vector dsb, algoritma apapun, yg tidak boleh adalah mengubah nilai langsung dari environment atau stats musuh atau stats anda sendiri menggunakan fungsi yg tdk dperkenankan
    //silahkan mengambil deltatime menggunakan Time.deltaTime, tdk masalah
    // kecurangan berakibat nilai = 0;

    private Tank tankController;
    public float maxDistanceToWall = 3f;
    public bool enemyDetected;
    private Vector3 currentFacingWall;
    bool isTurning;
    bool inFrontOfWall;

    public float thinkingDelay = 1;
    float thinkingTimer;

    public YasirTankFase tankFase;
    public YasirSearchingFase searchingFase;
    public YasirAttackingFase attackingFase;

    public float searchingFaseDuration = 3f;
    public float attackingFaseDuration = 6f;
    float searchingFaseTimer;
    float attackingFaseTimer;
    bool shooting;
    bool evadingWall;

    // Use this for initialization
    void Start () {
        tankController = GetComponent<Tank>();

        thinkingTimer = thinkingDelay;

        searchingFaseTimer = searchingFaseDuration;
    }

    // Update is called once per frame
    void Update()
    {
        tankController.rig.velocity = Vector2.zero; //disarankan jgn dihapus agar tank anda tidak jalan terus krn velocity, silahkan dihapus jika anda mengerti apa yg anda lakukan 

        enemyDetected = tankController._infIsEnemySeen;

        for (int i = 0; i < tankController._infLastposWall.Count; i++)
        {
            Vector3 wallPos = tankController._infLastposWall[i];
            Vector3 heading = wallPos - transform.position;
            if (Vector3.Dot(heading, tankController._infdirection.normalized) > 0.9 && Vector3.Distance(transform.position, wallPos) < maxDistanceToWall)
            {
                currentFacingWall = wallPos;
                inFrontOfWall = true;
                break;
            }
            else
            {
                inFrontOfWall = false;
            }
        }

        if (thinkingTimer > 0)
        {
            thinkingTimer -= Time.deltaTime;
        }
        else
        {
            if (enemyDetected)
            {
                tankFase = YasirTankFase.Attacking;
            } else if (!evadingWall)
            {
                tankFase = YasirTankFase.Searching;
            }

            thinkingTimer = thinkingDelay;
        }

        if (tankFase == YasirTankFase.Searching)
        {
            if (searchingFaseTimer > 0)
            {
                searchingFaseTimer -= Time.deltaTime;
            }
            else
            {
                searchingFaseTimer = searchingFaseDuration;

                if (searchingFase == YasirSearchingFase.Moving)
                {
                    searchingFase = YasirSearchingFase.Rotating;
                }
                else
                {
                    searchingFase = YasirSearchingFase.Moving;
                }
            }

            if (searchingFase == YasirSearchingFase.Moving)
            {
                if (!isTurning)
                {
                    tankController.Move(1);
                }

                if (!enemyDetected)
                {
                    if (inFrontOfWall)
                    {
                        tankController.Turn(1);

                        isTurning = true;
                    }
                    else
                    {
                        isTurning = false;
                    }
                }
                else if (enemyDetected)
                {
                    searchingFase = YasirSearchingFase.Moving;
                }   
            }
            else if (searchingFase == YasirSearchingFase.Rotating)
            {
                tankController.Turn(-1);
                if (enemyDetected)
                {
                    tankFase = YasirTankFase.Attacking;
                }
            }
        }
        else if(tankFase == YasirTankFase.Attacking)
        {
            if (shooting)
            {
                if (attackingFaseTimer > 0)
                {
                    attackingFaseTimer -= Time.deltaTime;
                }
                else
                {
                    attackingFaseTimer = attackingFaseDuration;

                    //if (attackingFase == YasirAttackingFase.StraightShoot)
                    //{
                    //    attackingFase = YasirAttackingFase.HitAndRun;
                    //}
                    //else
                    //{
                    //    attackingFase = YasirAttackingFase.StraightShoot;
                    //}

                    evadingWall = false;
                    shooting = false;
                }
            }

            if (attackingFase == YasirAttackingFase.StraightShoot)
            {
                Vector3 heading = tankController._infEnemyLastPos - transform.position;
                Vector3 perp = Vector3.Cross(transform.forward, heading);

                if (inFrontOfWall)
                {
                    heading = currentFacingWall - transform.position;
                    perp = Vector3.Cross(transform.forward, heading);
                    evadingWall = true;
                }

                float dir = Vector3.Dot(perp, transform.up);

                if (!inFrontOfWall)
                {
                    if (dir > 0.2f)
                    {
                        tankController.Turn(1);
                    }
                    else if (dir < -.2f)
                    {
                        tankController.Turn(-1);
                    }
                    else if (dir <= 0.2f && dir >= -0.2f)
                    {
                        tankController.Shoot();
                        shooting = true;
                    }
                }
                else
                {
                    if (dir > 0.2f)
                    {
                        tankController.Turn(-1);
                    }
                    else if (dir < -.2f)
                    {
                        tankController.Turn(1);
                    }

                    tankController.Move(1);
                }
            }
            //else if (attackingFase == YasirAttackingFase.HitAndRun)
            //{
            //    Vector3 heading = tankController._infEnemyLastPos - transform.position;
            //    Vector3 perp = Vector3.Cross(transform.forward, heading);

            //    float dir = Vector3.Dot(perp, transform.up);
                
            //    if (Vector3.Distance(transform.position, tankController._infEnemyLastPos) < maxDistanceToEnemy)
            //    {
            //        tankController.Move(1);
            //    }

            //    tankController.Shoot();
            //    shooting = true;
            //}
        }
    }
}

public enum YasirTankFase
{
    Searching,
    Attacking,
}

public enum YasirSearchingFase
{
    Moving,
    Rotating
}

public enum YasirAttackingFase
{
    StraightShoot,
}