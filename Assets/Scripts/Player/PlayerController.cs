using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Button buttonLeft;
    public Button buttonRight;
    public GameObject characterObject; // Referência para o objeto do personagem
    private Rigidbody2D rb;
    private Camera mainCamera;
    public bool isMagnet;
    public float magnetDuration = 20f; // Duração do powerup em segundos
    private float magnetTimer; // Tempo restante do powerup

    [SerializeField]
    public float moveSpeed = 5f;
    public float movementDistance = 1f; // Distância de movimento ao pressionar um botão

    private float targetPosition = 0f;
    private Transform characterTransform; // Referência para o componente Transform do characterObject

    public AudioClip collisionSound; // Som de colisão com o magnet

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        isMagnet = false;
        magnetTimer = 0f;

        // Adicionar eventos de clique aos botões
        buttonLeft.onClick.AddListener(OnButtonLeftPressed);
        buttonRight.onClick.AddListener(OnButtonRightPressed);

        characterTransform = characterObject.transform; // Obter o componente Transform do characterObject
    }

    void Update()
    {
        if (characterObject == null)
            return; // Sair do método se o characterObject for nulo ou destruído

        float currentX = characterObject.transform.position.x;

        // Determinar a direção do movimento com base na posição atual e na posição de destino
        float direction = Mathf.Sign(targetPosition - currentX);

        // Calcular a distância que o personagem deve se mover neste quadro
        float distanceToMove = direction * movementDistance * moveSpeed * Time.deltaTime;

        // Verificar se o movimento ultrapassaria a posição de destino
        float distanceToTarget = Mathf.Abs(targetPosition - currentX);
        if (Mathf.Abs(distanceToMove) > distanceToTarget)
        {
            // Se o movimento for maior que a distância para o destino, ajustar a distância para o destino
            distanceToMove = distanceToTarget * direction;
        }

        // Mover o personagem
        characterObject.transform.Translate(Vector2.right * distanceToMove);

        // Limitar a posição do personagem dentro dos limites da câmera
        float characterWidth = 0f;
        SpriteRenderer characterRenderer = characterObject.GetComponent<SpriteRenderer>();
        if (characterRenderer != null)
            characterWidth = characterRenderer.bounds.extents.x;

        float leftBound = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)).x + characterWidth;
        float rightBound = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, mainCamera.nearClipPlane)).x - characterWidth;

        Vector3 characterPosition = characterObject.transform.position;
        characterPosition.x = Mathf.Clamp(characterPosition.x, leftBound, rightBound);
        characterObject.transform.position = characterPosition;

        if (isMagnet)
        {
            if (magnetTimer > 0f)
            {
                magnetTimer -= Time.deltaTime;

                if (magnetTimer <= 0f)
                {
                    isMagnet = false;
                    // O tempo do powerup acabou, adicione qualquer código adicional que desejar
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerEnter == buttonLeft.gameObject)
        {
            targetPosition = characterObject.transform.position.x - movementDistance;
        }
        else if (eventData.pointerEnter == buttonRight.gameObject)
        {
            targetPosition = characterObject.transform.position.x + movementDistance;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    void OnButtonLeftPressed()
    {
        targetPosition = characterObject.transform.position.x - movementDistance;
        characterTransform.localScale = new Vector3(-0.3f, characterTransform.localScale.y, characterTransform.localScale.z);
    }

    void OnButtonRightPressed()
    {
        targetPosition = characterObject.transform.position.x + movementDistance;
        characterTransform.localScale = new Vector3(0.3f, characterTransform.localScale.y, characterTransform.localScale.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Magnet")
        {
            isMagnet = true;
            magnetTimer = magnetDuration;
            Destroy(collision.gameObject);

            if (collisionSound != null)
            {
                AudioSource.PlayClipAtPoint(collisionSound, transform.position);
            }
        }
    }
}
