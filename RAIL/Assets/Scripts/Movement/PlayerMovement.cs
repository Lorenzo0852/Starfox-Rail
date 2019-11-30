using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerMovement : MonoBehaviour
{
    #region Variable declaration
    private Transform playerModel;

    [Header("Settings")]
    public bool joystick = true;

    [Space]

    [Header("Parameters")]
    public float xySpeed = 18;
    public float lookSpeed = 340;
    public float forwardSpeed = 6;
    public float spinMovementX = 1;

    [Space]

    [Header("Public References")]
    public Transform aimTarget;
    public CinemachineDollyCart dolly;
    public Transform cameraParent;
    public Camera playerCamera;

    [Space]

    [Header("Particles")]
    public ParticleSystem trail;
    public ParticleSystem circle;
    public ParticleSystem barrel;
    public ParticleSystem stars;
    #endregion

    void Start()
    {
        playerModel = transform.GetChild(0);
        SetSpeed(forwardSpeed);
    }

    void Update()
    {
        //Se diferencia si se está utilizando un joystick o no, usando diferentes controles en función.
        float h = joystick ? Input.GetAxis("Horizontal") : Input.GetAxis("Mouse X");
        float v = joystick ? Input.GetAxis("Vertical") : Input.GetAxis("Mouse Y");
        //

        //Queremos manejar ciertos parámetros de movimientos continuamente.
        LocalMove(h, v, xySpeed);
        RotationLook(h,v, lookSpeed);
        HorizontalLean(playerModel, h, 80, .1f);
        //

        if (Input.GetButtonDown("Jump"))
            Boost(true);

        if (Input.GetButtonUp("Jump"))
            Boost(false);

        if (Input.GetButtonDown("Fire3"))
            Break(true);

        if (Input.GetButtonUp("Fire3"))
            Break(false);

        if (Input.GetButtonDown("TriggerL") || Input.GetButtonDown("TriggerR"))
        {
            int dir = Input.GetButtonDown("TriggerL") ? -1 : 1;
            QuickSpin(dir);
        }


    }

    /// <summary>
    /// Mueve la nave en los ejes X e Y según las intrucciones que le demos.
    /// </summary>
    /// <param name="x">Vector de posición horizontal X</param>
    /// <param name="y">Vector de posición vertical Y</param>
    /// <param name="speed">Velocidad de movimiento en los ejes X e Y</param>
    void LocalMove(float x, float y, float speed)
    {
        transform.localPosition += new Vector3(x, y, 0) * speed * Time.deltaTime;
        ClampPosition();
    }
    /// <summary>
    /// Fija la posición dentro de los límites visibles de la pantalla.
    /// </summary>
    void ClampPosition()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
    }

    /// <summary>
    /// Rota la nave si estamos moviéndola.
    /// </summary>
    /// <param name="h">Movimiento horizontal</param>
    /// <param name="v">Movimiento vertical</param>
    /// <param name="speed">Velocidad de rotación hacia la posición donde tenga que mirar.</param>
    void RotationLook(float h, float v, float speed)
    {
        aimTarget.parent.position = Vector3.zero;
        aimTarget.localPosition = new Vector3(h, v, 1);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(aimTarget.position), Mathf.Deg2Rad * speed * Time.deltaTime);
    }

    /// <summary>
    /// Gira la nave si únicamente nos estamos moviendo en el eje horizontal, al igual que un avión rota inclinando las alas al cambiar de dirección.
    /// </summary>
    /// <param name="target">Objeto que queremos rotar.</param>
    /// <param name="axis">Eje sobre el que queremos rotar.</param>
    /// <param name="leanLimit">Rotación máxima a alcanzar.</param>
    /// <param name="lerpTime">Tiempo que tarda en rotar.</param>
    void HorizontalLean(Transform target, float axis, float leanLimit, float lerpTime)
    {
        Vector3 targetEulerAngels = target.localEulerAngles;
        target.localEulerAngles = new Vector3(targetEulerAngels.x, targetEulerAngels.y, Mathf.LerpAngle(targetEulerAngels.z, -axis * leanLimit, lerpTime));
    }

    /// <summary>
    /// Dibuja la posición de aimTarget para debug.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(aimTarget.position, .5f);
        Gizmos.DrawSphere(aimTarget.position, .15f);

    }

    /// <summary>
    /// Maneja el "barrel roll", rota la nave 360º sobre si misma.
    /// </summary>
    /// <param name="dir">Dirección de la rotación.</param>
    public void QuickSpin(int dir)
    {
        if (!DOTween.IsTweening(playerModel))
        {
            playerModel.DOLocalRotate(new Vector3(playerModel.localEulerAngles.x, playerModel.localEulerAngles.y, 360 * -dir), .4f, RotateMode.LocalAxisAdd).SetEase(Ease.OutSine);
            transform.DOLocalMoveX(transform.localPosition.x + (spinMovementX * dir), 0.4f);
            barrel.Play();
        }
    }

    /// <summary>
    /// Establece la velocidad de movimiento.
    /// </summary>
    /// <param name="x"></param>
    void SetSpeed(float x)
    {
        dolly.m_Speed = x;
    }

    /// <summary>
    /// Cambia el zoom de la cámara.
    /// </summary>
    /// <param name="zoom">Zoom deseado.</param>
    /// <param name="duration">Tiempo que tardará en cambiar el zoom.</param>
    void SetCameraZoom(float zoom, float duration)
    {
        cameraParent.DOLocalMove(new Vector3(0, 0, zoom), duration);
    }

    /// <summary>
    /// Maneja la cantidad de distorsión del postprocesado.
    /// </summary>
    /// <param name="intensityValue">Intensidad de la distorsión.</param>
    void DistortionAmount(float intensityValue)
    {
        Camera.main.GetComponent<PostProcessVolume>().profile.GetSetting<LensDistortion>().intensity.value = intensityValue;
    }

    /// <summary>
    /// Cambia el FOV de la cámara.
    /// </summary>
    /// <param name="fov">Grados de visión deseados.</param>
    void FieldOfView(float fov)
    {
        cameraParent.GetComponentInChildren<CinemachineVirtualCamera>().m_Lens.FieldOfView = fov;
        playerCamera.fieldOfView = fov;
    }

    /// <summary>
    /// Maneja la cantidad de aberración cromática del postprocesado.
    /// </summary>
    /// <param name="intensityValue">Intensidad deseada.</param>
    void Chromatic(float intensityValue)
    {
        Camera.main.GetComponent<PostProcessVolume>().profile.GetSetting<ChromaticAberration>().intensity.value = intensityValue;
    }

    /// <summary>
    /// Maneja el aumento de velocidad.
    /// </summary>
    /// <param name="state">Estado que activa/desactiva el boost.</param>
    void Boost(bool state)
    {

        if (state)
        {
            cameraParent.GetComponentInChildren<CinemachineImpulseSource>().GenerateImpulse();
            trail.Play();
            circle.Play();
        }
        else
        {
            trail.Stop();
            circle.Stop();
        }
        trail.GetComponent<TrailRenderer>().emitting = state;

        float origFov = state ? 40 : 55;
        float endFov = state ? 55 : 40;
        float origChrom = state ? 0 : 1;
        float endChrom = state ? 1 : 0;
        float origDistortion = state ? 0 : -30;
        float endDistorton = state ? -30 : 0;
        float starsVel = state ? -20 : -1;
        float speed = state ? forwardSpeed * 2 : forwardSpeed;
        float zoom = state ? -7 : 0;

        DOVirtual.Float(origChrom, endChrom, .5f, Chromatic);
        DOVirtual.Float(origFov, endFov, .5f, FieldOfView);
        DOVirtual.Float(origDistortion, endDistorton, .5f, DistortionAmount);
        var pvel = stars.velocityOverLifetime;
        pvel.z = starsVel;

        DOVirtual.Float(dolly.m_Speed, speed, .15f, SetSpeed);
        SetCameraZoom(zoom, .4f);
    }

    /// <summary>
    /// Maneja el frenado.
    /// </summary>
    /// <param name="state">Estado que activa/desactiva el frenado.</param>
    void Break(bool state)
    {
        float speed = state ? forwardSpeed / 3 : forwardSpeed;
        float zoom = state ? 3 : 0;

        DOVirtual.Float(dolly.m_Speed, speed, .15f, SetSpeed);
        SetCameraZoom(zoom, .4f);
    }

    private void OnTriggerEnter(Collider other)
    {
        print("COLLIDER_HIT");
    }
    private void OnCollisionEnter(Collision collision)
    {
        print("COLLISION_HIT");
    }
}
