# ChessEduca - Motor de Xadrez com Poda Alpha-Beta

## Visão Geral

Este é um motor de xadrez educacional que demonstra os conceitos fundamentais usados em motores modernos como o Stockfish 17.1. O projeto foi criado para ajudar você a entender:

- **Algoritmo Minimax**: Base da busca em árvore de jogos
- **Poda Alpha-Beta**: Otimização que elimina ramos desnecessários
- **NNUE (Efficiently Updatable Neural Network)**: Rede neural para avaliação de posições
- **Avaliação Estática**: Análise de posições sem busca adicional

## Estrutura do Projeto

```
ChessEduca/
├── ChessTypes.cs        # Tipos básicos (Peça, Posição, Movimento)
├── Board.cs             # Representação do tabuleiro
├── MoveGenerator.cs     # Gerador de movimentos legais
├── Evaluation.cs        # Função de avaliação clássica
├── SearchEngine.cs      # Motor com Minimax e Alpha-Beta
├── SimpleNNUE.cs        # Implementação simplificada de NNUE
└── Program.cs           # Loop principal e demos
```

## Como Compilar

### Opção 1: Visual Studio
1. Abra `ChessEduca.slnx` no Visual Studio 2022
2. Pressione F5 para compilar e executar

### Opção 2: Linha de Comando
```bash
cd ChessEduca
dotnet build
dotnet run
```

### Opção 3: Script Batch
```bash
cd ChessEduca
compile.bat
```

## Como Executar

Após compilar, execute o programa e escolha uma opção:

1. **Jogo Automático**: O motor joga contra si mesmo
2. **Explicação Alpha-Beta**: Tutorial sobre poda Alpha-Beta
3. **Explicação NNUE**: Como funciona a rede neural
4. **Demo com Debug**: Visualização detalhada da busca
5. **Teste de Performance**: Comparação de algoritmos

## Conceitos Principais

### Poda Alpha-Beta

A poda Alpha-Beta é uma otimização do Minimax que elimina ramos da árvore que não podem influenciar o resultado final:

```
        MAX
       /   \
     MIN   MIN
     / \   / \
    3  12  8  ?  <- Este '?' pode ser podado!
```

**Como funciona:**
- **Alpha**: Melhor valor garantido para o maximizador
- **Beta**: Melhor valor garantido para o minimizador
- Quando Beta ≤ Alpha, podemos parar a busca (poda)

### Função Recursiva Principal

```csharp
private int AlphaBeta(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
{
    // Caso base
    if (depth == 0) return Evaluate(board);

    if (maximizingPlayer) {
        int maxEval = int.MinValue;
        foreach (var move in moves) {
            int eval = AlphaBeta(newBoard, depth-1, alpha, beta, false);
            maxEval = Math.Max(maxEval, eval);
            alpha = Math.Max(alpha, eval);
            if (beta <= alpha) break; // Poda Beta!
        }
        return maxEval;
    } else {
        int minEval = int.MaxValue;
        foreach (var move in moves) {
            int eval = AlphaBeta(newBoard, depth-1, alpha, beta, true);
            minEval = Math.Min(minEval, eval);
            beta = Math.Min(beta, eval);
            if (beta <= alpha) break; // Poda Alpha!
        }
        return minEval;
    }
}
```

### NNUE Simplificado

Nossa implementação de NNUE demonstra o conceito básico:

- **Entrada**: 768 features (6 peças × 2 cores × 64 casas)
- **Camada Oculta**: 32 neurônios com ReLU
- **Saída**: Score de avaliação

No Stockfish real:
- Rede muito maior (~40MB de pesos)
- Treinada em bilhões de posições
- Atualização incremental otimizada
- SIMD/AVX2 para paralelização

## Comparação com Stockfish

| Característica | ChessEduca | Stockfish 17.1 |
|----------------|------------|----------------|
| Profundidade de busca | 4-6 níveis | 20-40+ níveis |
| Nós por segundo | ~10K | ~100M+ |
| Tamanho NNUE | 32 neurônios | ~512+ neurônios |
| Transposition Tables | Não | Sim (GB de RAM) |
| Null Move Pruning | Não | Sim |
| Late Move Reductions | Não | Sim |
| Multithreading | Não | Sim (128+ cores) |

## Debug e Análise

O programa mostra informações detalhadas durante a execução:

- **Nós pesquisados**: Quantas posições foram analisadas
- **Podas realizadas**: Quantos ramos foram eliminados
- **Tempo de busca**: Duração da análise
- **Score**: Avaliação da posição (centipeões)
- **PV (Principal Variation)**: Sequência de melhores movimentos

## Experimentos Sugeridos

1. **Altere a profundidade**: Mude o valor em `Search(board, 4)` para ver o impacto
2. **Modifique a avaliação**: Ajuste os valores em `Evaluation.cs`
3. **Compare NNUE vs Clássica**: Use a opção 5 do menu
4. **Desative a poda**: Comente as linhas `if (beta <= alpha) break;`
5. **Ajuste os pesos NNUE**: Modifique `SimulateTrainedWeights()`

## Requisitos

- .NET 8.0 SDK ou superior
- Visual Studio 2022 (opcional)
- Windows 10/11

## Problemas Comuns

### Erro de compilação
- Certifique-se de ter o .NET 8.0 instalado
- Tente abrir no Visual Studio e compilar por lá

### Jogo muito lento
- Reduza a profundidade de busca para 3
- Desative NNUE (use apenas avaliação clássica)

### Movimentos estranhos
- Normal em profundidades baixas
- O motor não tem tabelas de finais nem livro de aberturas

## Recursos Adicionais

- [Stockfish no GitHub](https://github.com/official-stockfish/Stockfish)
- [Chess Programming Wiki](https://www.chessprogramming.org/)
- [Alpha-Beta Pruning](https://en.wikipedia.org/wiki/Alpha-beta_pruning)
- [NNUE Documentation](https://github.com/official-stockfish/nnue-pytorch)

## Licença

Este projeto é educacional e de código aberto. Use livremente para aprender!