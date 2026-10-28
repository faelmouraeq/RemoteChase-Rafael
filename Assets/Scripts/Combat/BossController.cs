using UnityEngine;

namespace RemoteChase.Combat
{
    [AddComponentMenu("Remote Chase/Boss Controller")]
    public class BossController : MonoBehaviour
    {
        [Header("Partes do Chefão")]
        [Tooltip("Arraste aqui o script BossPart do canhão esquerdo.")]
        public BossPart leftCannon;

        [Tooltip("Arraste aqui o script BossPart do canhão direito.")]
        public BossPart rightCannon;

        [Tooltip("O script BossPart do núcleo central.")]
        public BossPart corePart;

        [Header("Status da Batalha")]
        public bool cannonsDestroyed = false;

        void Start()
        {
            // Garante que o núcleo começa protegido se houver canhões ativos
            if (corePart != null)
            {
                corePart.isShieldingCore = true;
            }
        }

        void Update()
        {
            // Se os canhões já foram destruídos, não precisa ficar checando toda hora
            if (cannonsDestroyed) return;

            // Verificação contínua automática (opcional, mas mantemos o método abaixo para chamadas diretas)
            CheckCanonsDestroyed();
        }

        // Método chamado pelo BossPart quando um canhão é destruído
        public void CheckCanonsDestroyed()
        {
            if (cannonsDestroyed) return;

            bool leftDead = (leftCannon == null || !leftCannon.gameObject.activeSelf);
            bool rightDead = (rightCannon == null || !rightCannon.gameObject.activeSelf);

            if (leftDead && rightDead)
            {
                UnlockCore();
            }
        }

        void UnlockCore()
        {
            cannonsDestroyed = true;
            Debug.Log("🎉 AMBOS OS CANHÕES FORAM DESTRUÍDOS! O Núcleo Central está vulnerável!");

            if (corePart != null)
            {
                // Libera o núcleo para tomar dano
                corePart.isShieldingCore = false;
            }
        }
    }
}
