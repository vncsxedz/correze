# Configurar o personagem MINIMO no Unity

Esta integração usa os arquivos MINIMO que já estão instalados no seu projeto Unity. Eles ficam locais no seu computador e não são enviados para este repositório.

## 1. Abra o projeto atualizado

Baixe/atualize o branch `codex/minimo-character-bridge` e abra o projeto com Unity 6.3. Aguarde o Unity terminar a compilação.

## 2. Configure o GameArt

No painel Project, abra:

`Assets/Art/GameArt.asset`

No Inspector, preencha a seção **Optional humanoid runner**:

- **Runner Model Prefab:** `Assets/Minimo/Character/SavedPrefabs/Characters/Default/Character-01`
- **Runner Idle:** `Assets/Minimo/Character/Animations/Anims/MoveIdleLeft`
- **Runner Run:** `Assets/Minimo/Character/Animations/Anims/MoveRun`
- **Runner Jump:** `Assets/Minimo/Character/Animations/Anims/DashJump` (ou `JumpRight`)
- **Runner Slide:** `Assets/Minimo/Character/Animations/Anims/CrouchWalk`
- **Runner Fall:** `Assets/Minimo/Character/Animations/Anims/HardFall`

Arraste cada arquivo do painel Project para o campo correspondente no Inspector. Se o nome de algum clip variar, use o clip equivalente na mesma pasta.

## 3. Ajuste os clips que repetem

Abra o modelo/arquivo de animação no Project e, no Inspector, verifique as opções de importação dos clips:

- Deixe **Loop Time** ligado em Idle e Run.
- Deixe **Loop Time** desligado em Fall.
- Jump e Slide podem ficar ligados ou desligados conforme o movimento do clip; teste o resultado no jogo.

## 4. Confira no jogo

Abra `Assets/Scenes/Gauntlet.unity`, aguarde a compilação e pressione Play. O modelo só substitui o visual atual quando o prefab, Animator e os clips de corrida, salto, deslize e queda estiverem atribuídos. Se algo estiver faltando, o personagem procedural existente continua visível.

A câmera, as faixas, os controles, a física, as colisões, a pontuação e a progressão continuam usando os sistemas existentes do jogo. A animação conduz apenas o visual do modelo.

## Se a pasta MINIMO não aparecer

Confirme que você está no projeto local onde importou o pacote. A pasta `Assets/Minimo` pode ter outro nome se você escolheu um caminho diferente ao importar. Pesquise no campo de busca do painel Project por `Character-01`, `MoveRun`, `DashJump`, `CrouchWalk` e `HardFall`.

Os assets do pacote são dependências locais; este repositório contém apenas os campos de configuração e o código de integração.
