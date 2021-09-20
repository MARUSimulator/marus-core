using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiverIconTurn : MonoBehaviour
{
    public Transform m_Camera;
    public Transform m_Head;

    RectTransform m_CameraIcon;

    // Start is called before the first frame update
    void Start()
    {
        if (!m_Camera || !m_Head)
            this.enabled = false;
        m_CameraIcon = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        m_CameraIcon.localRotation = Quaternion.Lerp(m_CameraIcon.rotation, Quaternion.Euler(0, 0, -m_Camera.eulerAngles.y + m_Head.eulerAngles.y), Time.deltaTime * 10);
    }
}
