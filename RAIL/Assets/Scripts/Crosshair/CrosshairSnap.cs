using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CrosshairSnap : MonoBehaviour
{
    [Header("Assign manually")]
    /// <summary>
    /// Usa el objeto del jugador para identificar a los enemigos en frente de él.
    /// </summary>
    public GameObject player;

    /// <summary>
    /// Crosshair in-world object the object with this script will follow.
    /// </summary>
    public GameObject crosshairDummy;

    [Header("Auto assigned")]
    /// <summary>
    /// Main crosshair that will snap to the enemies.
    /// </summary>
    public GameObject snappableCrosshair;

    /// <summary>
    /// Secondary crosshair that will apear when the main one is snapped;
    /// </summary>
    public GameObject secondaryCrosshair;

    /// <summary>
    /// Objeto que define la dirección de apuntado.
    /// </summary>
    public Transform raycaster;

    [Header("Settings")]

    public float distanceToSnap = 125;

    public float maxBehindEngageDistance = 10f;

    #region Private variables

    /// <summary>
    /// List with all enemies on the scene.
    /// </summary>
    List<GameObject> allEnemies;

    /// <summary>
    /// Time elapsed until it scans all enemies.
    /// </summary>
    float timeToScanEnemies = 3f;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        snappableCrosshair = transform.GetChild(0).gameObject;

        secondaryCrosshair = transform.GetChild(1).gameObject;
        secondaryCrosshair.SetActive(false);

        raycaster = transform.GetChild(2).gameObject.transform;

        allEnemies = new List<GameObject>();

        InvokeRepeating("AddEnemiesToList", 0, timeToScanEnemies);
    }

    // Update is called once per frame
    void Update()
    {
        secondaryCrosshair.transform.position = crosshairDummy.transform.position;
        snappableCrosshair.transform.position = crosshairDummy.transform.position;
        DetectEnemies(allEnemies);
        Snap();
    }

    /// <summary>
    /// Funcion que detecta enemigos y selecciona los que estén en visión.
    /// </summary>
    /// <param name="enemies">Lista de enemigos.</param>
    private void DetectEnemies(List<GameObject> enemies)
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            //Calcula la dirección entre la posición del jugador y la de todos los enemigos
            Vector3 direction = player.transform.position - enemies[i].transform.position;
            //Crea un rayo empezando en el jugador, siguiendo la dirección anterior.
            Ray ray = new Ray(enemies[i].transform.position, direction);
            //Crea un raycast y guarda los datos de sus impactos en "hit".
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
        
                //Verifica cosas.
            if(Mathf.Abs(hit.distance-Vector3.Distance(enemies[i].transform.position, player.transform.position)) < 2
                //Verifica si se encuentra dentro de la distancia de snap.
                && hit.distance < distanceToSnap
                //Verifica si está delante o detrás.
                && Vector3.Dot(snappableCrosshair.transform.forward, enemies[i].transform.position - snappableCrosshair.transform.position) > maxBehindEngageDistance)
            {
                //Cambia el estado del enemigo para indicarle que es visible.
                enemies[i].GetComponent<Enemy>().snap = Enemy.CanSeePlayer.Seen;
                Debug.DrawRay(enemies[i].transform.position, direction, Color.green);
            }
            else
            {
                //Si no se cumple nada de lo anterior, señala al enemigo que no está siendo visible.
                enemies[i].GetComponent<Enemy>().snap = Enemy.CanSeePlayer.Unseen;
                Debug.DrawRay(enemies[i].transform.position, direction, Color.red);
            }
        }
    }

    void Snap()
    {
        RaycastHit hit;
        Physics.Raycast(crosshairDummy.transform.position, (raycaster.transform.position - Camera.main.transform.position), out hit);
        Debug.DrawRay(crosshairDummy.transform.position, (transform.position - Camera.main.transform.position) * 100, Color.blue);
        if (hit.transform != null &&
            hit.transform.gameObject.tag == "Enemy" &&
            hit.transform.GetComponent<Enemy>().snap == Enemy.CanSeePlayer.Seen)
        {
            snappableCrosshair.transform.DOMove(hit.transform.position, 0.2f);
            secondaryCrosshair.SetActive(true);
        }
        else
        {
            secondaryCrosshair.SetActive(false);
            snappableCrosshair.transform.position = crosshairDummy.transform.position;
        }
    }

    void AddEnemiesToList() {
        allEnemies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
    }
}
