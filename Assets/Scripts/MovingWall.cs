using UnityEngine;

public class MovingWall : MonoBehaviour
{
    public float speed = 3f;
    
    private float _distance = 2.5f;
    private Vector3 _startPos;
    private bool _movingUp = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float moveStep = speed * Time.deltaTime;

        if (_movingUp)
        {
            transform.position += new Vector3(0f, 0f, moveStep);
            if (transform.position.z >= _startPos.z + _distance)
            {
                _movingUp = false;
            }
        }
        else
        {
            transform.position -= new Vector3(0f, 0f, moveStep);
            if (transform.position.z <= _startPos.z - _distance)
            {
                _movingUp = true;
            }
        }
    }
}
