namespace ChessEduca;

public class SearchEngine
{
    private int nodesSearched;
    private int pruningCuts;
    private DateTime searchStartTime;
    private int maxSearchTime = 5000; // 5 segundos por movimento
    public bool UseNNUE { get; set; } = false;
    private SimpleNNUE? nnue;

    public SearchEngine()
    {
        nnue = new SimpleNNUE();
    }

    public class SearchResult
    {
        public Move BestMove { get; set; }
        public int Score { get; set; }
        public int Depth { get; set; }
        public int NodesSearched { get; set; }
        public int PruningCuts { get; set; }
        public double TimeElapsed { get; set; }
        public List<Move> PrincipalVariation { get; set; } = new List<Move>();
    }

    public SearchResult Search(Board board, int maxDepth)
    {
        nodesSearched = 0;
        pruningCuts = 0;
        searchStartTime = DateTime.Now;

        SearchResult result = new SearchResult();
        Move bestMove = new Move();
        int bestScore = board.SideToMove == Color.White ? int.MinValue : int.MaxValue;

        // Iterative Deepening - aumenta gradualmente a profundidade
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            if (IsTimeUp()) break;

            Move currentBestMove = new Move();
            int currentBestScore = board.SideToMove == Color.White ? int.MinValue : int.MaxValue;
            List<Move> pv = new List<Move>();

            // Gerar e ordenar movimentos
            MoveGenerator moveGen = new MoveGenerator(board);
            List<Move> moves = moveGen.GenerateMoves();

            if (moves.Count == 0)
            {
                // Xeque-mate ou empate
                if (board.IsInCheck(board.SideToMove))
                {
                    result.Score = board.SideToMove == Color.White ? -100000 : 100000;
                    Console.WriteLine("XEQUE-MATE!");
                }
                else
                {
                    result.Score = 0;
                    Console.WriteLine("EMPATE!");
                }
                return result;
            }

            // Ordenar movimentos para melhor eficiência da poda
            moves = OrderMoves(board, moves);

            foreach (var move in moves)
            {
                if (IsTimeUp()) break;

                Board newBoard = new Board(board);
                newBoard.MakeMove(move);

                // Chamada recursiva do Alpha-Beta
                int score = AlphaBeta(
                    newBoard,
                    depth - 1,
                    int.MinValue,
                    int.MaxValue,
                    board.SideToMove == Color.Black,
                    pv
                );

                if (board.SideToMove == Color.White && score > currentBestScore)
                {
                    currentBestScore = score;
                    currentBestMove = move;
                }
                else if (board.SideToMove == Color.Black && score < currentBestScore)
                {
                    currentBestScore = score;
                    currentBestMove = move;
                }
            }

            // Atualizar melhor movimento se a busca completou nesta profundidade
            if (!IsTimeUp())
            {
                bestMove = currentBestMove;
                bestScore = currentBestScore;
                result.PrincipalVariation = new List<Move> { bestMove };
                result.PrincipalVariation.AddRange(pv);

                // Debug: Mostrar progresso da busca
                Console.WriteLine($"Profundidade {depth}: Melhor movimento = {bestMove}, " +
                                $"Score = {bestScore}, Nós = {nodesSearched}, " +
                                $"Podas = {pruningCuts}");
            }
        }

        result.BestMove = bestMove;
        result.Score = bestScore;
        result.NodesSearched = nodesSearched;
        result.PruningCuts = pruningCuts;
        result.TimeElapsed = (DateTime.Now - searchStartTime).TotalSeconds;
        result.Depth = maxDepth;

        return result;
    }

    // ALGORITMO MINIMAX COM PODA ALPHA-BETA
    // Esta é a função recursiva principal que você queria entender!
    private int AlphaBeta(Board board, int depth, int alpha, int beta, bool maximizingPlayer, List<Move> pv)
    {
        nodesSearched++;

        // Caso base: profundidade 0 ou fim de jogo
        if (depth == 0 || IsTimeUp())
        {
            // Usar NNUE ou avaliação clássica
            if (UseNNUE && nnue != null)
            {
                return nnue.Evaluate(board);
            }
            else
            {
                return QuiescenceSearch(board, alpha, beta, maximizingPlayer, 5);
            }
        }

        MoveGenerator moveGen = new MoveGenerator(board);
        List<Move> moves = moveGen.GenerateMoves();

        // Verificar xeque-mate ou empate
        if (moves.Count == 0)
        {
            if (board.IsInCheck(board.SideToMove))
            {
                // Xeque-mate: retorna score muito alto/baixo baseado em quem está em xeque
                return maximizingPlayer ? -100000 + depth : 100000 - depth;
            }
            else
            {
                // Empate
                return 0;
            }
        }

        // Ordenar movimentos para melhor eficiência da poda
        moves = OrderMoves(board, moves);

        if (maximizingPlayer)
        {
            // MAXIMIZING PLAYER (geralmente Brancas)
            int maxEval = int.MinValue;
            Move bestMove = new Move();

            foreach (var move in moves)
            {
                Board newBoard = new Board(board);
                newBoard.MakeMove(move);

                List<Move> childPV = new List<Move>();
                int eval = AlphaBeta(newBoard, depth - 1, alpha, beta, false, childPV);

                if (eval > maxEval)
                {
                    maxEval = eval;
                    bestMove = move;
                    pv.Clear();
                    pv.Add(bestMove);
                    pv.AddRange(childPV);
                }

                // PODA ALPHA
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                {
                    pruningCuts++; // Contador de podas para debug
                    break; // Beta cutoff - podar este ramo
                }
            }
            return maxEval;
        }
        else
        {
            // MINIMIZING PLAYER (geralmente Pretas)
            int minEval = int.MaxValue;
            Move bestMove = new Move();

            foreach (var move in moves)
            {
                Board newBoard = new Board(board);
                newBoard.MakeMove(move);

                List<Move> childPV = new List<Move>();
                int eval = AlphaBeta(newBoard, depth - 1, alpha, beta, true, childPV);

                if (eval < minEval)
                {
                    minEval = eval;
                    bestMove = move;
                    pv.Clear();
                    pv.Add(bestMove);
                    pv.AddRange(childPV);
                }

                // PODA BETA
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                {
                    pruningCuts++; // Contador de podas para debug
                    break; // Alpha cutoff - podar este ramo
                }
            }
            return minEval;
        }
    }

    // Quiescence Search - busca apenas capturas para evitar efeito horizonte
    private int QuiescenceSearch(Board board, int alpha, int beta, bool maximizingPlayer, int depth)
    {
        nodesSearched++;

        // Avaliação estática da posição
        int standPat = Evaluation.Evaluate(board);

        if (depth == 0)
            return standPat;

        if (maximizingPlayer)
        {
            if (standPat >= beta)
                return beta;
            if (alpha < standPat)
                alpha = standPat;

            MoveGenerator moveGen = new MoveGenerator(board);
            List<Move> moves = moveGen.GenerateMoves();

            // Filtrar apenas capturas
            moves = moves.Where(m => !board.GetPiece(m.To).IsEmpty).ToList();
            moves = OrderMoves(board, moves);

            foreach (var move in moves)
            {
                Board newBoard = new Board(board);
                newBoard.MakeMove(move);

                int score = QuiescenceSearch(newBoard, alpha, beta, false, depth - 1);

                if (score >= beta)
                {
                    pruningCuts++;
                    return beta;
                }
                if (score > alpha)
                    alpha = score;
            }
            return alpha;
        }
        else
        {
            if (standPat <= alpha)
                return alpha;
            if (beta > standPat)
                beta = standPat;

            MoveGenerator moveGen = new MoveGenerator(board);
            List<Move> moves = moveGen.GenerateMoves();

            // Filtrar apenas capturas
            moves = moves.Where(m => !board.GetPiece(m.To).IsEmpty).ToList();
            moves = OrderMoves(board, moves);

            foreach (var move in moves)
            {
                Board newBoard = new Board(board);
                newBoard.MakeMove(move);

                int score = QuiescenceSearch(newBoard, alpha, beta, true, depth - 1);

                if (score <= alpha)
                {
                    pruningCuts++;
                    return alpha;
                }
                if (score < beta)
                    beta = score;
            }
            return beta;
        }
    }

    // Ordenar movimentos para melhor eficiência da poda alpha-beta
    private List<Move> OrderMoves(Board board, List<Move> moves)
    {
        // Avaliar cada movimento rapidamente
        for (int i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            move.Score = Evaluation.QuickEvaluate(board, move);
            moves[i] = move;
        }

        // Ordenar por score (melhor primeiro)
        return moves.OrderByDescending(m => m.Score).ToList();
    }

    private bool IsTimeUp()
    {
        return (DateTime.Now - searchStartTime).TotalMilliseconds > maxSearchTime;
    }

    // Explicação didática da poda Alpha-Beta
    public void ExplainAlphaBetaPruning()
    {
        Console.WriteLine("\n=== EXPLICAÇÃO DA PODA ALPHA-BETA ===\n");
        Console.WriteLine("A poda Alpha-Beta é uma otimização do algoritmo Minimax que elimina");
        Console.WriteLine("ramos da árvore de busca que não podem influenciar a decisão final.\n");

        Console.WriteLine("CONCEITOS FUNDAMENTAIS:");
        Console.WriteLine("------------------------");
        Console.WriteLine("1. ALPHA: Melhor valor que o maximizador já encontrou");
        Console.WriteLine("   - Começa em -∞");
        Console.WriteLine("   - Representa o 'piso' para o maximizador");
        Console.WriteLine();
        Console.WriteLine("2. BETA: Melhor valor que o minimizador já encontrou");
        Console.WriteLine("   - Começa em +∞");
        Console.WriteLine("   - Representa o 'teto' para o minimizador");
        Console.WriteLine();

        Console.WriteLine("COMO FUNCIONA A PODA:");
        Console.WriteLine("---------------------");
        Console.WriteLine("1. PODA BETA (no nó maximizador):");
        Console.WriteLine("   - Se encontramos um valor ≥ beta");
        Console.WriteLine("   - O minimizador acima nunca escolherá este ramo");
        Console.WriteLine("   - Podemos parar de explorar os filhos restantes");
        Console.WriteLine();
        Console.WriteLine("2. PODA ALPHA (no nó minimizador):");
        Console.WriteLine("   - Se encontramos um valor ≤ alpha");
        Console.WriteLine("   - O maximizador acima nunca escolherá este ramo");
        Console.WriteLine("   - Podemos parar de explorar os filhos restantes");
        Console.WriteLine();

        Console.WriteLine("EXEMPLO VISUAL:");
        Console.WriteLine("--------------");
        Console.WriteLine("        MAX");
        Console.WriteLine("       /   \\");
        Console.WriteLine("     MIN   MIN");
        Console.WriteLine("     / \\   / \\");
        Console.WriteLine("    3  12  8  ? <- Este '?' pode ser podado!");
        Console.WriteLine();
        Console.WriteLine("Se o MIN da esquerda retorna 3, o MAX tem garantido pelo menos 3.");
        Console.WriteLine("Se o MIN da direita encontra 8 primeiro, sabe que retornará no máximo 8.");
        Console.WriteLine("Mas o MAX já tem 3 da esquerda, então nunca escolheria este ramo.");
        Console.WriteLine("Logo, não precisa avaliar o '?' - isso é uma poda alpha!");
        Console.WriteLine();

        Console.WriteLine("VANTAGENS:");
        Console.WriteLine("----------");
        Console.WriteLine("- Reduz drasticamente o número de nós avaliados");
        Console.WriteLine("- Mantém o mesmo resultado do Minimax puro");
        Console.WriteLine("- Permite buscar mais profundamente no mesmo tempo");
        Console.WriteLine("- No melhor caso: O(b^(d/2)) em vez de O(b^d)");
        Console.WriteLine();

        Console.WriteLine("No Stockfish e outros motores modernos, a poda Alpha-Beta é");
        Console.WriteLine("combinada com outras técnicas como:");
        Console.WriteLine("- Iterative Deepening");
        Console.WriteLine("- Move Ordering (ordenação de movimentos)");
        Console.WriteLine("- Transposition Tables (cache de posições)");
        Console.WriteLine("- Null Move Pruning");
        Console.WriteLine("- Late Move Reductions");
        Console.WriteLine();
    }
}