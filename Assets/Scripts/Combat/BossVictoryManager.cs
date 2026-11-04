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
        [Tooltip("Nome exato da cena de vitória para onde o jogo vai após derrotar o chefe.")]
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

            // Aguarda 2.5 segundos para o jogador apreciar a vitória e chama a transição
            Invoke(nameof(TriggerVictorySequence), 2.5f);
        }

        void TriggerVictorySequence()
        {
            Debug.Log($"✨ Carregando a cena de vitória: {victorySceneName}");
            SceneManager.LoadScene(victorySceneName);
        }
    }
}
