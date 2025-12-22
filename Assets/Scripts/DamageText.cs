// DamageText.cs
// CREATE THIS SEVENTH - No dependencies
// Creates floating damage numbers in combat

using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeSpeed = 1f;
    
    private TextMesh textMesh;
    private float timer;
    
    public void Initialize(float damage, bool isCrit)
    {
        textMesh = gameObject.AddComponent<TextMesh>();
        textMesh.text = damage.ToString("F0");
        textMesh.fontSize = isCrit ? 60 : 40;
        textMesh.color = isCrit ? Color.yellow : Color.white;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }
        
        transform.position += new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(0, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }
        
        if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a = 1f - (timer / lifetime);
            textMesh.color = color;
        }
        
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}