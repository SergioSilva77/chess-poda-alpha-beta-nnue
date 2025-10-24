namespace ChessEduca;

public class Evaluation
{
    // Valores materiais das peças (em centipeões - 1 peão = 100)
    private static readonly int[] PieceValues = { 0, 100, 320, 330, 500, 900, 20000 };

    // Tabelas de posição para cada tipo de peça (Piece-Square Tables)
    // Valores positivos são bons para brancas, do ponto de vista das brancas
    private static readonly int[,] PawnTable = {
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        { 50, 50, 50, 50, 50, 50, 50, 50 },
        { 10, 10, 20, 30, 30, 20, 10, 10 },
        {  5,  5, 10, 25, 25, 10,  5,  5 },
        {  0,  0,  0, 20, 20,  0,  0,  0 },
        {  5, -5,-10,  0,  0,-10, -5,  5 },
        {  5, 10, 10,-20,-20, 10, 10,  5 },
        {  0,  0,  0,  0,  0,  0,  0,  0 }
    };

    private static readonly int[,] KnightTable = {
        {-50,-40,-30,-30,-30,-30,-40,-50 },
        {-40,-20,  0,  0,  0,  0,-20,-40 },
        {-30,  0, 10, 15, 15, 10,  0,-30 },
        {-30,  5, 15, 20, 20, 15,  5,-30 },
        {-30,  0, 15, 20, 20, 15,  0,-30 },
        {-30,  5, 10, 15, 15, 10,  5,-30 },
        {-40,-20,  0,  5,  5,  0,-20,-40 },
        {-50,-40,-30,-30,-30,-30,-40,-50 }
    };

    private static readonly int[,] BishopTable = {
        {-20,-10,-10,-10,-10,-10,-10,-20 },
        {-10,  0,  0,  0,  0,  0,  0,-10 },
        {-10,  0,  5, 10, 10,  5,  0,-10 },
        {-10,  5,  5, 10, 10,  5,  5,-10 },
        {-10,  0, 10, 10, 10, 10,  0,-10 },
        {-10, 10, 10, 10, 10, 10, 10,-10 },
        {-10,  5,  0,  0,  0,  0,  5,-10 },
        {-20,-10,-10,-10,-10,-10,-10,-20 }
    };

    private static readonly int[,] RookTable = {
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        {  5, 10, 10, 10, 10, 10, 10,  5 },
        { -5,  0,  0,  0,  0,  0,  0, -5 },
        { -5,  0,  0,  0,  0,  0,  0, -5 },
        { -5,  0,  0,  0,  0,  0,  0, -5 },
        { -5,  0,  0,  0,  0,  0,  0, -5 },
        { -5,  0,  0,  0,  0,  0,  0, -5 },
        {  0,  0,  0,  5,  5,  0,  0,  0 }
    };

    private static readonly int[,] QueenTable = {
        {-20,-10,-10, -5, -5,-10,-10,-20 },
        {-10,  0,  0,  0,  0,  0,  0,-10 },
        {-10,  0,  5,  5,  5,  5,  0,-10 },
        { -5,  0,  5,  5,  5,  5,  0, -5 },
        {  0,  0,  5,  5,  5,  5,  0, -5 },
        {-10,  5,  5,  5,  5,  5,  0,-10 },
        {-10,  0,  5,  0,  0,  0,  0,-10 },
        {-20,-10,-10, -5, -5,-10,-10,-20 }
    };

    private static readonly int[,] KingMiddleGameTable = {
        {-30,-40,-40,-50,-50,-40,-40,-30 },
        {-30,-40,-40,-50,-50,-40,-40,-30 },
        {-30,-40,-40,-50,-50,-40,-40,-30 },
        {-30,-40,-40,-50,-50,-40,-40,-30 },
        {-20,-30,-30,-40,-40,-30,-30,-20 },
        {-10,-20,-20,-20,-20,-20,-20,-10 },
        { 20, 20,  0,  0,  0,  0, 20, 20 },
        { 20, 30, 10,  0,  0, 10, 30, 20 }
    };

    private static readonly int[,] KingEndGameTable = {
        {-50,-40,-30,-20,-20,-30,-40,-50 },
        {-30,-20,-10,  0,  0,-10,-20,-30 },
        {-30,-10, 20, 30, 30, 20,-10,-30 },
        {-30,-10, 30, 40, 40, 30,-10,-30 },
        {-30,-10, 30, 40, 40, 30,-10,-30 },
        {-30,-10, 20, 30, 30, 20,-10,-30 },
        {-30,-30,  0,  0,  0,  0,-30,-30 },
        {-50,-30,-30,-30,-30,-30,-30,-50 }
    };

    // Avaliação estática da posição
    public static int Evaluate(Board board)
    {
        int score = 0;

        // Material e posição
        int materialWhite = 0;
        int materialBlack = 0;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Position pos = new Position(row, col);
                Piece piece = board.GetPiece(pos);

                if (piece.IsEmpty) continue;

                int pieceValue = PieceValues[(int)piece.Type];
                int positionBonus = GetPositionBonus(piece, row, col, IsEndGame(board));

                if (piece.Color == Color.White)
                {
                    materialWhite += pieceValue;
                    score += pieceValue + positionBonus;
                }
                else
                {
                    materialBlack += pieceValue;
                    score -= pieceValue + positionBonus;
                }
            }
        }

        // Bônus por mobilidade (número de movimentos legais)
        MoveGenerator moveGen = new MoveGenerator(board);
        List<Move> moves = moveGen.GenerateMoves();
        int mobility = moves.Count;
        score += board.SideToMove == Color.White ? mobility * 10 : -mobility * 10;

        // Penalidade por rei em xeque
        if (board.IsInCheck(board.SideToMove))
        {
            score += board.SideToMove == Color.White ? -50 : 50;
        }

        // Bônus por controle do centro
        score += EvaluateCenterControl(board);

        // Bônus por estrutura de peões
        score += EvaluatePawnStructure(board);

        // Retorna do ponto de vista de quem está jogando
        return board.SideToMove == Color.White ? score : -score;
    }

    private static int GetPositionBonus(Piece piece, int row, int col, bool isEndGame)
    {
        // Para peças pretas, espelhar o tabuleiro verticalmente
        int effectiveRow = piece.Color == Color.White ? row : 7 - row;

        int bonus = piece.Type switch
        {
            PieceType.Pawn => PawnTable[effectiveRow, col],
            PieceType.Knight => KnightTable[effectiveRow, col],
            PieceType.Bishop => BishopTable[effectiveRow, col],
            PieceType.Rook => RookTable[effectiveRow, col],
            PieceType.Queen => QueenTable[effectiveRow, col],
            PieceType.King => isEndGame ?
                KingEndGameTable[effectiveRow, col] :
                KingMiddleGameTable[effectiveRow, col],
            _ => 0
        };

        return bonus;
    }

    private static bool IsEndGame(Board board)
    {
        // Considera fim de jogo quando há poucas peças pesadas
        int heavyPieces = 0;
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece piece = board.GetPiece(new Position(row, col));
                if (piece.Type == PieceType.Queen || piece.Type == PieceType.Rook)
                {
                    heavyPieces++;
                }
            }
        }
        return heavyPieces <= 2;
    }

    private static int EvaluateCenterControl(Board board)
    {
        int score = 0;
        Position[] centerSquares = {
            new Position(3, 3), new Position(3, 4),
            new Position(4, 3), new Position(4, 4)
        };

        foreach (var pos in centerSquares)
        {
            Piece piece = board.GetPiece(pos);
            if (!piece.IsEmpty)
            {
                score += piece.Color == Color.White ? 10 : -10;
            }

            // Bônus por controlar o centro com peões
            if (board.IsSquareAttacked(pos, Color.White))
                score += 5;
            if (board.IsSquareAttacked(pos, Color.Black))
                score -= 5;
        }

        return score;
    }

    private static int EvaluatePawnStructure(Board board)
    {
        int score = 0;

        // Penalizar peões dobrados e isolados
        for (int col = 0; col < 8; col++)
        {
            int whitePawnsInColumn = 0;
            int blackPawnsInColumn = 0;

            for (int row = 0; row < 8; row++)
            {
                Piece piece = board.GetPiece(new Position(row, col));
                if (piece.Type == PieceType.Pawn)
                {
                    if (piece.Color == Color.White)
                        whitePawnsInColumn++;
                    else
                        blackPawnsInColumn++;
                }
            }

            // Penalizar peões dobrados
            if (whitePawnsInColumn > 1)
                score -= (whitePawnsInColumn - 1) * 10;
            if (blackPawnsInColumn > 1)
                score += (blackPawnsInColumn - 1) * 10;
        }

        return score;
    }

    // Avaliação rápida para ordenação de movimentos
    public static int QuickEvaluate(Board board, Move move)
    {
        int score = 0;
        Piece movingPiece = board.GetPiece(move.From);
        Piece capturedPiece = board.GetPiece(move.To);

        // MVV-LVA (Most Valuable Victim - Least Valuable Attacker)
        if (!capturedPiece.IsEmpty)
        {
            score = PieceValues[(int)capturedPiece.Type] - PieceValues[(int)movingPiece.Type] / 10;
        }

        // Bônus por promoção
        if (move.PromotionPiece != PieceType.None)
        {
            score += PieceValues[(int)move.PromotionPiece];
        }

        // Bônus por mover para o centro
        int centerDistance = Math.Abs(move.To.Row - 3) + Math.Abs(move.To.Col - 3);
        score -= centerDistance * 2;

        return score;
    }
}