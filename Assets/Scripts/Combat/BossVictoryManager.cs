using UnityEngine;
using UnityEngine.SceneManagement;

namespace RemoteChase.Combat
{
    [AddComponentMenu("Remote Chase/Boss Victory Manager")]
    public class BossVictoryManager : MonoBehaviour
    {
        [Header("Referências")]
        [Tooltip("O script BossPart do Núcleo Central do chefe.")]
        public BossPart coreBossPart;

        [Header("Configuração de Fim de Jogo")]
        public string victorySceneName = "VictoryScene";

        private bool isDefeated = false;

        void Update()
        {
            if (isDefeated || coreBossPart == null) return;

            // Se o núcleo foi desativado (vida <= 0), o chefe foi derrotado!
            if (!coreBossPart.gameObject.activeSelf)
            {
                HandleBossDefeat();
            }
        }

        void HandleBossDefeat()
        {
            isDefeated = true;
            Debug.Log("🏆 CHEFE DERROTADO! A Fase 2 foi vencida!");

            // Aqui você poderá chamar sua tela de vitória no futuro
            // Exemplo: SceneManager.LoadScene(victorySceneName);

            // Por enquanto, chamamos uma rotina de transição ou pausa
            Invoke(nameof(TriggerVictorySequence), 2.5f);
        }

        void TriggerVictorySequence()
        {
            Debug.Log("✨ Transição para a Tela de Vitória acionada com sucesso!");
            // Coloque aqui a lógica do seu painel de UI de vitória ou carregamento de cena.
        }
    }
}
