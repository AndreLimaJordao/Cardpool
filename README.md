# 🗃️ Cardpool

**Cardpool** é um aplicativo de terminal leve e eficiente projetado para gerenciar sessões de estudo, treinos e aquecimentos. Utilizando conceitos de **repetição espaçada** e **time-boxing**, ele ajuda você a focar na *execução* prática das tarefas, e não apenas na memorização.

Originalmente criado para gerenciar a rotina de aquecimento do [Drawabox](https://drawabox.com/), o Cardpool é perfeito para qualquer habilidade que exija consistência, como prática de instrumentos musicais, exercícios físicos, ou programação.

---

## Por que o Cardpool é diferente?

Diferente de aplicativos de flashcards tradicionais (como o Anki), que focam em "lembrar ou não lembrar", o Cardpool foca em **fazer**. 

Você não avalia se decorou uma carta. Você diz ao programa: *"Eu tenho 15 minutos para praticar"*, e o Cardpool monta a sessão puxando cartas da sua pilha de tarefas com base na prioridade de cada uma e no tempo que você tem disponível.

## Principais Funcionalidades

* **Sessões Dinâmicas (Time-boxing):** Filtre suas tarefas por *tags* e defina o tempo total que você tem disponível. O app seleciona os exercícios que cabem exatamente nesse limite.
* **Barra de Progresso com Checkpoints:** Durante a sessão, uma barra visual mostra o tempo decorrido e exibe marcadores (`╬`) indicando o momento exato de trocar para o próximo exercício.
* **O Sistema de "Pilha" (Pile) e Prioridades:** Cartões com maior prioridade aparecem mais vezes na sua rotação. Cartões concluídos vão para o final da fila, garantindo variedade e repetição orgânica sem algoritmos confusos.
* **Embaralhamento Inteligente:** O comando de reshuffle randomiza a pilha ativamente evitando que o mesmo tipo de tarefa apareça em sequência.
* **Gamificação e Logs:** Todo tempo de sessão concluído é registrado. O aplicativo calcula sua pontuação (1 minuto = 1 ponto) para manter você motivado com metas diárias, semanais e mensais.

## Casos de Uso

* **Arte e Desenho (Drawabox):** Gerencie seus 15 minutos de aquecimento diário alternando entre linhas, elipses e caixas de perspectiva.
* **Música:** Rotacione a prática de escalas, arpejos, aquecimento vocal e repertório sem estourar o tempo do seu treino.
* **Exercícios Físicos:** Crie rotinas de alongamento ou calistenia em casa, usando o bipe sonoro do terminal para trocar de postura.

## Como Instalar e Rodar

O projeto foi construído em **C# (.NET)**. Para rodá-lo, você precisará ter o SDK do .NET instalado na sua máquina.

1. **Pré-requisito:** Instale o [.NET SDK](https://dotnet.microsoft.com/download) (versão 6.0 ou superior recomendada).
2. **Clone o repositório:**
   ```bash
   git clone [https://github.com/AndreLimaJordao/Cardpool.git](https://github.com/AndreLimaJordao/Cardpool.git)
   cd Cardpool
   ```
3. Execute o aplicativo:
   ```bash
   dotnet run
   ```

## Como Usar (Guia Rápido)

1. Vá em "Add Exercise/WarmUp Card" para criar suas tarefas. Defina um nome, uma prioridade (ex: 1 a 5), o tempo necessário (em minutos) e tags separadas por vírgula (ex: linhas, aquecimento).
2. Vá em "Start Study/WarmUp Session".
3. (Opcional) Digite uma tag para filtrar os exercícios.
4. Digite quantos minutos você tem disponíveis (ex: 15).
5. Confirme o plano gerado e siga o timer na tela!

## Licença

Este projeto é de código aberto e está disponível sob a Licença MIT. Sinta-se à vontade para clonar, modificar e usar.
   
