namespace ChessEduca;

// NNUE Simplificado - Efficiently Updatable Neural Network
// Similar ao usado no Stockfish, mas muito mais simples para fins educacionais
public class SimpleNNUE
{
    // Dimensões da rede neural
    private const int INPUT_SIZE = 768;  // 6 tipos de peça * 2 cores * 64 casas
    private const int HIDDEN_SIZE = 32;  // Camada oculta pequena para simplicidade
    private const int OUTPUT_SIZE = 1;   // Score de avaliação

    // Pesos da rede (inicializados aleatoriamente)
    private float[,] weightsInputToHidden;
    private float[] biasHidden;
    private float[] weightsHiddenToOutput;
    private float biasOutput;

    private Random random = new Random(42); // Seed fixo para reproducibilidade

    public SimpleNNUE()
    {
        InitializeWeights();
    }

    private void InitializeWeights()
    {
        // Inicializar pesos com valores pequenos aleatórios (Xavier initialization simplificada)
        weightsInputToHidden = new float[INPUT_SIZE, HIDDEN_SIZE];
        biasHidden = new float[HIDDEN_SIZE];
        weightsHiddenToOutput = new float[HIDDEN_SIZE];

        // Inicialização dos pesos da primeira camada
        float scale1 = (float)Math.Sqrt(2.0 / INPUT_SIZE);
        for (int i = 0; i < INPUT_SIZE; i++)
        {
            for (int j = 0; j < HIDDEN_SIZE; j++)
            {
                weightsInputToHidden[i, j] = (float)(random.NextDouble() * 2 - 1) * scale1;
            }
        }

        // Bias da camada oculta
        for (int i = 0; i < HIDDEN_SIZE; i++)
        {
            biasHidden[i] = 0.0f;
        }

        // Pesos da camada de saída
        float scale2 = (float)Math.Sqrt(2.0 / HIDDEN_SIZE);
        for (int i = 0; i < HIDDEN_SIZE; i++)
        {
            weightsHiddenToOutput[i] = (float)(random.NextDouble() * 2 - 1) * scale2;
        }

        biasOutput = 0.0f;

        // Simular pesos "treinados" ajustando alguns valores importantes
        SimulateTrainedWeights();
    }

    private void SimulateTrainedWeights()
    {
        // Ajustar alguns pesos para simular treinamento
        // Isso faz a rede valorizar mais certas características do jogo

        // Aumentar importância de peças centrais
        for (int pieceType = 0; pieceType < 6; pieceType++)
        {
            for (int color = 0; color < 2; color++)
            {
                // Casas centrais (e4, d4, e5, d5)
                int[] centralSquares = { 27, 28, 35, 36 };
                foreach (int square in centralSquares)
                {
                    int inputIndex = GetInputIndex(pieceType, color, square);
                    if (inputIndex < INPUT_SIZE)
                    {
                        // Aumentar peso para conexões com neurônios que valorizam o centro
                        for (int h = 0; h < 8; h++) // Primeiros 8 neurônios para centro
                        {
                            weightsInputToHidden[inputIndex, h] *= 1.5f;
                        }
                    }
                }
            }
        }

        // Ajustar pesos de saída para refletir valores de peças
        // Neurônios 0-7: Centro
        // Neurônios 8-15: Material
        // Neurônios 16-23: Segurança do rei
        // Neurônios 24-31: Estrutura de peões

        for (int i = 0; i < 8; i++)
        {
            weightsHiddenToOutput[i] = 10.0f;  // Centro importante
            weightsHiddenToOutput[i + 8] = 20.0f;  // Material muito importante
            weightsHiddenToOutput[i + 16] = 15.0f; // Segurança do rei importante
            weightsHiddenToOutput[i + 24] = 5.0f;  // Estrutura de peões
        }
    }

    // Converter posição do tabuleiro em vetor de features
    private float[] BoardToFeatures(Board board)
    {
        float[] features = new float[INPUT_SIZE];

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Position pos = new Position(row, col);
                Piece piece = board.GetPiece(pos);

                if (!piece.IsEmpty)
                {
                    int square = row * 8 + col;
                    int inputIndex = GetInputIndex((int)piece.Type - 1, (int)piece.Color, square);

                    if (inputIndex < INPUT_SIZE)
                    {
                        features[inputIndex] = 1.0f;
                    }
                }
            }
        }

        // Adicionar features adicionais baseadas no estado do jogo
        // Normalizar para o intervalo [0, 1]

        // Feature: lado que joga
        features[0] = board.SideToMove == Color.White ? 1.0f : -1.0f;

        // Feature: direitos de roque
        for (int i = 0; i < 4; i++)
        {
            features[i + 1] = board.CastlingRights[i] ? 1.0f : 0.0f;
        }

        // Feature: en passant
        features[5] = board.EnPassantSquare.HasValue ? 1.0f : 0.0f;

        return features;
    }

    private int GetInputIndex(int pieceType, int color, int square)
    {
        // Mapear (tipo_peça, cor, casa) para índice no vetor de entrada
        // pieceType: 0-5 (peão a rei)
        // color: 0-1 (branco, preto)
        // square: 0-63
        return pieceType * 128 + color * 64 + square;
    }

    // Forward pass da rede neural
    public int Evaluate(Board board)
    {
        // 1. Converter tabuleiro em features
        float[] input = BoardToFeatures(board);

        // 2. Camada oculta com ativação ReLU
        float[] hidden = new float[HIDDEN_SIZE];
        for (int j = 0; j < HIDDEN_SIZE; j++)
        {
            float sum = biasHidden[j];
            for (int i = 0; i < INPUT_SIZE; i++)
            {
                sum += input[i] * weightsInputToHidden[i, j];
            }
            // ReLU activation
            hidden[j] = Math.Max(0, sum);
        }

        // 3. Camada de saída
        float output = biasOutput;
        for (int i = 0; i < HIDDEN_SIZE; i++)
        {
            output += hidden[i] * weightsHiddenToOutput[i];
        }

        // 4. Converter para centipeões e aplicar perspectiva
        int score = (int)(output * 100);

        // Adicionar componente clássico de avaliação para melhorar o jogo
        // (já que nossa rede não é realmente treinada)
        int classicalEval = GetSimpleEvaluation(board);
        score = (score + classicalEval * 3) / 4; // 25% NNUE, 75% clássico

        // Retornar do ponto de vista de quem joga
        return board.SideToMove == Color.White ? score : -score;
    }

    // Avaliação simples para complementar a NNUE
    private int GetSimpleEvaluation(Board board)
    {
        int score = 0;
        int[] pieceValues = { 0, 100, 320, 330, 500, 900, 20000 };

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Position pos = new Position(row, col);
                Piece piece = board.GetPiece(pos);

                if (!piece.IsEmpty)
                {
                    int value = pieceValues[(int)piece.Type];
                    if (piece.Color == Color.White)
                        score += value;
                    else
                        score -= value;

                    // Bônus por peças no centro
                    if (row >= 3 && row <= 4 && col >= 3 && col <= 4)
                    {
                        int centerBonus = 10;
                        if (piece.Type == PieceType.Pawn)
                            centerBonus = 20;
                        score += piece.Color == Color.White ? centerBonus : -centerBonus;
                    }
                }
            }
        }

        return score;
    }

    public void ExplainNNUE()
    {
        Console.WriteLine("\n=== EXPLICAÇÃO DO NNUE (EFFICIENTLY UPDATABLE NEURAL NETWORK) ===\n");
        Console.WriteLine("NNUE é a tecnologia de rede neural usada no Stockfish desde 2020.\n");

        Console.WriteLine("CARACTERÍSTICAS PRINCIPAIS:");
        Console.WriteLine("---------------------------");
        Console.WriteLine("1. ARQUITETURA:");
        Console.WriteLine("   - Entrada: Representação esparsa do tabuleiro (768 features)");
        Console.WriteLine("   - Camada oculta: 32 neurônios (Stockfish usa ~512)");
        Console.WriteLine("   - Saída: Score de avaliação em centipeões");
        Console.WriteLine();

        Console.WriteLine("2. EFFICIENTLY UPDATABLE:");
        Console.WriteLine("   - Atualização incremental quando peças se movem");
        Console.WriteLine("   - Não recalcula toda a rede a cada posição");
        Console.WriteLine("   - Mantém acumuladores para eficiência");
        Console.WriteLine();

        Console.WriteLine("3. FEATURES DE ENTRADA:");
        Console.WriteLine($"   - {INPUT_SIZE} inputs = 6 tipos de peça × 2 cores × 64 casas");
        Console.WriteLine("   - Representação 'one-hot' esparsa");
        Console.WriteLine("   - Features adicionais: roque, en passant, etc.");
        Console.WriteLine();

        Console.WriteLine("4. TREINAMENTO (no Stockfish real):");
        Console.WriteLine("   - Treinado em bilhões de posições");
        Console.WriteLine("   - Usa self-play e bases de dados de GMs");
        Console.WriteLine("   - Aprendizado por reforço + supervisionado");
        Console.WriteLine();

        Console.WriteLine("5. VANTAGENS SOBRE AVALIAÇÃO CLÁSSICA:");
        Console.WriteLine("   - Captura padrões complexos automaticamente");
        Console.WriteLine("   - Melhor avaliação de estruturas de peões");
        Console.WriteLine("   - Entende compensação material");
        Console.WriteLine("   - Avalia melhor posições desequilibradas");
        Console.WriteLine();

        Console.WriteLine("NOSSA IMPLEMENTAÇÃO SIMPLIFICADA:");
        Console.WriteLine("---------------------------------");
        Console.WriteLine($"- Rede pequena: {INPUT_SIZE} → {HIDDEN_SIZE} → 1");
        Console.WriteLine("- Pesos pseudo-aleatórios (não treinados)");
        Console.WriteLine("- Combina com avaliação clássica (75/25)");
        Console.WriteLine("- Demonstra o conceito sem o custo computacional");
        Console.WriteLine();

        Console.WriteLine("NO STOCKFISH REAL:");
        Console.WriteLine("------------------");
        Console.WriteLine("- Rede muito maior (~40MB de pesos)");
        Console.WriteLine("- Pesos treinados por milhares de horas");
        Console.WriteLine("- Atualização incremental super otimizada");
        Console.WriteLine("- SIMD/AVX2 para paralelização");
        Console.WriteLine("- Múltiplas redes para diferentes fases do jogo");
        Console.WriteLine();
    }
}