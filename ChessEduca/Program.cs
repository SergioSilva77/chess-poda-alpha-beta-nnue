using ChessEduca;
using System;

class Program
{
    static void ClearScreen()
    {
        // Por segurança, apenas adiciona linhas em branco
        Console.WriteLine("\n\n===========================================\n");
    }

    static void Main(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("     MOTOR DE XADREZ COM ALPHA-BETA       ");
        Console.WriteLine("     Exemplo Educacional - Estilo Stockfish");
        Console.WriteLine("===========================================\n");

        // Menu de opções
        Console.WriteLine("Escolha uma opção:");
        Console.WriteLine("1. Jogo automático (motor vs motor)");
        Console.WriteLine("2. Explicação da Poda Alpha-Beta");
        Console.WriteLine("3. Explicação do NNUE");
        Console.WriteLine("4. Demonstração com debug detalhado");
        Console.WriteLine("5. Teste rápido de performance");
        Console.Write("\nOpção: ");

        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                RunAutoPlay();
                break;
            case "2":
                ExplainAlphaBeta();
                break;
            case "3":
                ExplainNNUE();
                break;
            case "4":
                RunDebugDemo();
                break;
            case "5":
                RunPerformanceTest();
                break;
            default:
                RunAutoPlay();
                break;
        }

        Console.WriteLine("\nPressione qualquer tecla para sair...");
        try { Console.ReadKey(); } catch { }
    }

    static void RunAutoPlay()
    {
        ClearScreen();
        Console.WriteLine("=== JOGO AUTOMÁTICO - MOTOR vs MOTOR ===\n");
        Console.WriteLine("O motor jogará contra si mesmo.");
        Console.WriteLine("Profundidade de busca: 4");
        Console.WriteLine("Algoritmo: Minimax com Poda Alpha-Beta");
        Console.WriteLine("Avaliação: Clássica + NNUE simplificado\n");
        Console.WriteLine("Pressione qualquer tecla para começar...");
        try { Console.ReadKey(); } catch { }

        Board board = new Board();
        SearchEngine engineWhite = new SearchEngine();
        SearchEngine engineBlack = new SearchEngine();

        // Configurar se usa NNUE
        engineWhite.UseNNUE = false; // Brancas usa avaliação clássica
        engineBlack.UseNNUE = true;  // Pretas usa NNUE

        int moveCount = 0;
        int maxMoves = 100; // Limite de movimentos para evitar jogos infinitos

        while (moveCount < maxMoves)
        {
            ClearScreen();
            Console.WriteLine($"=== MOVIMENTO {moveCount + 1} ===\n");
            board.Display();

            // Verificar fim de jogo
            MoveGenerator moveGen = new MoveGenerator(board);
            var legalMoves = moveGen.GenerateMoves();

            if (legalMoves.Count == 0)
            {
                if (board.IsInCheck(board.SideToMove))
                {
                    Console.WriteLine($"\nXEQUE-MATE! {(board.SideToMove == Color.White ? "Pretas" : "Brancas")} vencem!");
                }
                else
                {
                    Console.WriteLine("\nEMPATE por afogamento!");
                }
                break;
            }

            // Verificar regra dos 50 movimentos
            if (board.HalfMoveClock >= 100)
            {
                Console.WriteLine("\nEMPATE pela regra dos 50 movimentos!");
                break;
            }

            // Motor calcula o melhor movimento
            SearchEngine currentEngine = board.SideToMove == Color.White ? engineWhite : engineBlack;
            string engineType = board.SideToMove == Color.White ? "Clássica" : "NNUE";

            Console.WriteLine($"\n{(board.SideToMove == Color.White ? "Brancas" : "Pretas")} pensando... (Avaliação: {engineType})");

            var result = currentEngine.Search(board, 4); // Profundidade 4

            if (result.BestMove.From.Row == 0 && result.BestMove.From.Col == 0 &&
                result.BestMove.To.Row == 0 && result.BestMove.To.Col == 0)
            {
                Console.WriteLine("\nNenhum movimento válido encontrado!");
                break;
            }

            Console.WriteLine($"\nMelhor movimento: {result.BestMove}");
            Console.WriteLine($"Avaliação: {result.Score / 100.0:F2} (do ponto de vista de quem joga)");
            Console.WriteLine($"Nós pesquisados: {result.NodesSearched:N0}");
            Console.WriteLine($"Podas Alpha-Beta: {result.PruningCuts:N0}");
            Console.WriteLine($"Tempo: {result.TimeElapsed:F2}s");

            // Fazer o movimento
            board.MakeMove(result.BestMove);
            moveCount++;

            //Console.WriteLine("\nPressione qualquer tecla para continuar...");
            //try { Console.ReadKey(); } catch { }
        }
        
        Console.WriteLine("\n=== FIM DO JOGO ===");
        Console.WriteLine($"Total de movimentos: {moveCount}");
    }

    static void RunDebugDemo()
    {
        ClearScreen();
        Console.WriteLine("=== DEMONSTRAÇÃO COM DEBUG DETALHADO ===\n");
        Console.WriteLine("Vamos analisar uma posição específica mostrando:");
        Console.WriteLine("- Árvore de busca");
        Console.WriteLine("- Quando ocorrem as podas");
        Console.WriteLine("- Valores Alpha e Beta\n");

        // Criar uma posição simples para demonstração
        Board board = new Board();
        board.SetupInitialPosition();

        // Fazer alguns movimentos para simplificar a posição
        board.MakeMove(new Move(new Position(6, 4), new Position(4, 4))); // e4
        board.MakeMove(new Move(new Position(1, 4), new Position(3, 4))); // e5
        board.MakeMove(new Move(new Position(7, 6), new Position(5, 5))); // Nf3

        Console.WriteLine("Posição atual:");
        board.Display();

        Console.WriteLine("\nAnalisando com profundidade 3...\n");

        // Criar um motor com debug
        SearchEngineDebug debugEngine = new SearchEngineDebug();
        var result = debugEngine.SearchWithDebug(board, 3);

        Console.WriteLine($"\n=== RESULTADO FINAL ===");
        Console.WriteLine($"Melhor movimento: {result.BestMove}");
        Console.WriteLine($"Avaliação: {result.Score / 100.0:F2}");
        Console.WriteLine($"Total de nós: {result.NodesSearched}");
        Console.WriteLine($"Total de podas: {result.PruningCuts}");
        Console.WriteLine($"Eficiência da poda: {(result.PruningCuts * 100.0 / result.NodesSearched):F1}%");
    }

    static void RunPerformanceTest()
    {
        ClearScreen();
        Console.WriteLine("=== TESTE DE PERFORMANCE ===\n");
        Console.WriteLine("Comparando busca COM e SEM poda Alpha-Beta\n");

        Board board = new Board();
        board.SetupInitialPosition();

        // Teste 1: Com poda Alpha-Beta
        Console.WriteLine("Teste 1: COM Poda Alpha-Beta (profundidade 5)");
        SearchEngine engineWithPruning = new SearchEngine();
        var start = DateTime.Now;
        var result1 = engineWithPruning.Search(board, 5);
        var elapsed1 = (DateTime.Now - start).TotalSeconds;

        Console.WriteLine($"  Tempo: {elapsed1:F2}s");
        Console.WriteLine($"  Nós pesquisados: {result1.NodesSearched:N0}");
        Console.WriteLine($"  Podas realizadas: {result1.PruningCuts:N0}");
        Console.WriteLine($"  Melhor movimento: {result1.BestMove}");

        // Teste 2: Simulação sem poda (profundidade menor)
        Console.WriteLine("\nTeste 2: Simulação SEM Poda (profundidade 3)");
        int nodesWithoutPruning = CalculateNodesWithoutPruning(board, 3);
        Console.WriteLine($"  Nós que seriam pesquisados: ~{nodesWithoutPruning:N0}");
        Console.WriteLine($"  Redução com poda: {((1 - (double)result1.NodesSearched / nodesWithoutPruning) * 100):F1}%");

        // Teste 3: Comparação NNUE vs Clássica
        Console.WriteLine("\nTeste 3: Comparação NNUE vs Avaliação Clássica");

        SearchEngine engineClassic = new SearchEngine { UseNNUE = false };
        start = DateTime.Now;
        var resultClassic = engineClassic.Search(board, 4);
        var elapsedClassic = (DateTime.Now - start).TotalSeconds;

        SearchEngine engineNNUE = new SearchEngine { UseNNUE = true };
        start = DateTime.Now;
        var resultNNUE = engineNNUE.Search(board, 4);
        var elapsedNNUE = (DateTime.Now - start).TotalSeconds;

        Console.WriteLine($"\n  Avaliação Clássica:");
        Console.WriteLine($"    Tempo: {elapsedClassic:F2}s");
        Console.WriteLine($"    Melhor movimento: {resultClassic.BestMove}");
        Console.WriteLine($"    Score: {resultClassic.Score / 100.0:F2}");

        Console.WriteLine($"\n  NNUE:");
        Console.WriteLine($"    Tempo: {elapsedNNUE:F2}s");
        Console.WriteLine($"    Melhor movimento: {resultNNUE.BestMove}");
        Console.WriteLine($"    Score: {resultNNUE.Score / 100.0:F2}");
    }

    static int CalculateNodesWithoutPruning(Board board, int depth)
    {
        // Estimativa: assumindo fator de ramificação médio de 35
        return (int)Math.Pow(35, depth);
    }

    static void ExplainAlphaBeta()
    {
        ClearScreen();
        SearchEngine engine = new SearchEngine();
        engine.ExplainAlphaBetaPruning();
    }

    static void ExplainNNUE()
    {
        ClearScreen();
        SimpleNNUE nnue = new SimpleNNUE();
        nnue.ExplainNNUE();
    }
}

// Versão do SearchEngine com debug detalhado
public class SearchEngineDebug : SearchEngine
{
    private int debugDepth = 0;

    public SearchResult SearchWithDebug(Board board, int maxDepth)
    {
        Console.WriteLine("LEGENDA:");
        Console.WriteLine("  [MAX] = Nó maximizador (quer o maior valor)");
        Console.WriteLine("  [MIN] = Nó minimizador (quer o menor valor)");
        Console.WriteLine("  α = Alpha (melhor garantido para MAX)");
        Console.WriteLine("  β = Beta (melhor garantido para MIN)");
        Console.WriteLine("  ✂️ = Poda realizada\n");
        Console.WriteLine("----------------------------------------\n");

        return Search(board, maxDepth);
    }
}
