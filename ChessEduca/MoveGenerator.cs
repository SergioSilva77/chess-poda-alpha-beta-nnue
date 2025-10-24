namespace ChessEduca;

public class MoveGenerator
{
    private Board board;
    private List<Move> moves;

    public MoveGenerator(Board board)
    {
        this.board = board;
        this.moves = new List<Move>();
    }

    public List<Move> GenerateMoves()
    {
        moves.Clear();

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Position from = new Position(row, col);
                Piece piece = board.GetPiece(from);

                if (piece.IsEmpty || piece.Color != board.SideToMove)
                    continue;

                GeneratePieceMoves(from, piece);
            }
        }

        // Filtrar movimentos ilegais (que deixam o rei em xeque)
        List<Move> legalMoves = new List<Move>();
        foreach (var move in moves)
        {
            Board testBoard = new Board(board);
            testBoard.MakeMove(move);
            if (!testBoard.IsInCheck(board.SideToMove))
            {
                legalMoves.Add(move);
            }
        }

        return legalMoves;
    }

    private void GeneratePieceMoves(Position from, Piece piece)
    {
        switch (piece.Type)
        {
            case PieceType.Pawn:
                GeneratePawnMoves(from, piece);
                break;
            case PieceType.Knight:
                GenerateKnightMoves(from, piece);
                break;
            case PieceType.Bishop:
                GenerateSlidingMoves(from, piece, new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) });
                break;
            case PieceType.Rook:
                GenerateSlidingMoves(from, piece, new[] { (-1, 0), (1, 0), (0, -1), (0, 1) });
                break;
            case PieceType.Queen:
                GenerateSlidingMoves(from, piece, new[] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) });
                break;
            case PieceType.King:
                GenerateKingMoves(from, piece);
                break;
        }
    }

    private void GeneratePawnMoves(Position from, Piece piece)
    {
        int direction = piece.Color == Color.White ? -1 : 1;
        int startRow = piece.Color == Color.White ? 6 : 1;
        int promotionRow = piece.Color == Color.White ? 0 : 7;

        // Movimento para frente (1 casa)
        Position oneSquare = new Position(from.Row + direction, from.Col);
        if (oneSquare.IsValid && board.GetPiece(oneSquare).IsEmpty)
        {
            if (oneSquare.Row == promotionRow)
            {
                // Promoções
                moves.Add(new Move(from, oneSquare, PieceType.Queen));
                moves.Add(new Move(from, oneSquare, PieceType.Rook));
                moves.Add(new Move(from, oneSquare, PieceType.Bishop));
                moves.Add(new Move(from, oneSquare, PieceType.Knight));
            }
            else
            {
                moves.Add(new Move(from, oneSquare));
            }

            // Movimento para frente (2 casas) do início
            if (from.Row == startRow)
            {
                Position twoSquares = new Position(from.Row + 2 * direction, from.Col);
                if (board.GetPiece(twoSquares).IsEmpty)
                {
                    moves.Add(new Move(from, twoSquares));
                }
            }
        }

        // Capturas diagonais
        for (int colOffset = -1; colOffset <= 1; colOffset += 2)
        {
            Position capture = new Position(from.Row + direction, from.Col + colOffset);
            if (!capture.IsValid) continue;

            Piece target = board.GetPiece(capture);

            // Captura normal
            if (!target.IsEmpty && target.Color != piece.Color)
            {
                if (capture.Row == promotionRow)
                {
                    // Promoções com captura
                    moves.Add(new Move(from, capture, PieceType.Queen));
                    moves.Add(new Move(from, capture, PieceType.Rook));
                    moves.Add(new Move(from, capture, PieceType.Bishop));
                    moves.Add(new Move(from, capture, PieceType.Knight));
                }
                else
                {
                    moves.Add(new Move(from, capture));
                }
            }

            // En passant
            if (board.EnPassantSquare.HasValue && capture.Equals(board.EnPassantSquare.Value))
            {
                moves.Add(new Move(from, capture));
            }
        }
    }

    private void GenerateKnightMoves(Position from, Piece piece)
    {
        int[] rowOffsets = { -2, -2, -1, -1, 1, 1, 2, 2 };
        int[] colOffsets = { -1, 1, -2, 2, -2, 2, -1, 1 };

        for (int i = 0; i < 8; i++)
        {
            Position to = new Position(from.Row + rowOffsets[i], from.Col + colOffsets[i]);
            if (!to.IsValid) continue;

            Piece target = board.GetPiece(to);
            if (target.IsEmpty || target.Color != piece.Color)
            {
                moves.Add(new Move(from, to));
            }
        }
    }

    private void GenerateSlidingMoves(Position from, Piece piece, (int, int)[] directions)
    {
        foreach (var (rowDir, colDir) in directions)
        {
            Position current = new Position(from.Row + rowDir, from.Col + colDir);

            while (current.IsValid)
            {
                Piece target = board.GetPiece(current);

                if (target.IsEmpty)
                {
                    moves.Add(new Move(from, current));
                }
                else
                {
                    if (target.Color != piece.Color)
                    {
                        moves.Add(new Move(from, current));
                    }
                    break; // Não pode pular peças
                }

                current = new Position(current.Row + rowDir, current.Col + colDir);
            }
        }
    }

    private void GenerateKingMoves(Position from, Piece piece)
    {
        // Movimentos normais do rei
        for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            for (int colOffset = -1; colOffset <= 1; colOffset++)
            {
                if (rowOffset == 0 && colOffset == 0) continue;

                Position to = new Position(from.Row + rowOffset, from.Col + colOffset);
                if (!to.IsValid) continue;

                Piece target = board.GetPiece(to);
                if (target.IsEmpty || target.Color != piece.Color)
                {
                    moves.Add(new Move(from, to));
                }
            }
        }

        // Roque
        if (!board.IsInCheck(piece.Color))
        {
            int row = piece.Color == Color.White ? 7 : 0;

            // Roque curto
            int castleKingIndex = piece.Color == Color.White ? 0 : 2;
            if (board.CastlingRights[castleKingIndex] && from.Row == row && from.Col == 4)
            {
                if (board.GetPiece(new Position(row, 5)).IsEmpty &&
                    board.GetPiece(new Position(row, 6)).IsEmpty &&
                    board.GetPiece(new Position(row, 7)).Type == PieceType.Rook)
                {
                    // Verificar se as casas intermediárias não estão sob ataque
                    if (!board.IsSquareAttacked(new Position(row, 5), piece.Color == Color.White ? Color.Black : Color.White) &&
                        !board.IsSquareAttacked(new Position(row, 6), piece.Color == Color.White ? Color.Black : Color.White))
                    {
                        moves.Add(new Move(from, new Position(row, 6)));
                    }
                }
            }

            // Roque longo
            int castleQueenIndex = piece.Color == Color.White ? 1 : 3;
            if (board.CastlingRights[castleQueenIndex] && from.Row == row && from.Col == 4)
            {
                if (board.GetPiece(new Position(row, 3)).IsEmpty &&
                    board.GetPiece(new Position(row, 2)).IsEmpty &&
                    board.GetPiece(new Position(row, 1)).IsEmpty &&
                    board.GetPiece(new Position(row, 0)).Type == PieceType.Rook)
                {
                    // Verificar se as casas intermediárias não estão sob ataque
                    if (!board.IsSquareAttacked(new Position(row, 3), piece.Color == Color.White ? Color.Black : Color.White) &&
                        !board.IsSquareAttacked(new Position(row, 2), piece.Color == Color.White ? Color.Black : Color.White))
                    {
                        moves.Add(new Move(from, new Position(row, 2)));
                    }
                }
            }
        }
    }
}