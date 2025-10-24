namespace ChessEduca;

public class Board
{
    private Piece[,] squares;
    public Color SideToMove { get; set; }
    public int HalfMoveClock { get; set; } // Para regra dos 50 movimentos
    public int FullMoveNumber { get; set; }
    public bool[] CastlingRights { get; set; } // WK, WQ, BK, BQ
    public Position? EnPassantSquare { get; set; }

    public Board()
    {
        squares = new Piece[8, 8];
        CastlingRights = new bool[4];
        SetupInitialPosition();
    }

    // Construtor de cópia para fazer/desfazer movimentos
    public Board(Board other)
    {
        squares = new Piece[8, 8];
        Array.Copy(other.squares, squares, 64);
        SideToMove = other.SideToMove;
        HalfMoveClock = other.HalfMoveClock;
        FullMoveNumber = other.FullMoveNumber;
        CastlingRights = (bool[])other.CastlingRights.Clone();
        EnPassantSquare = other.EnPassantSquare;
    }

    public void SetupInitialPosition()
    {
        // Limpar tabuleiro
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                squares[i, j] = new Piece(PieceType.None, Color.White);

        // Peças brancas
        squares[7, 0] = new Piece(PieceType.Rook, Color.White);
        squares[7, 1] = new Piece(PieceType.Knight, Color.White);
        squares[7, 2] = new Piece(PieceType.Bishop, Color.White);
        squares[7, 3] = new Piece(PieceType.Queen, Color.White);
        squares[7, 4] = new Piece(PieceType.King, Color.White);
        squares[7, 5] = new Piece(PieceType.Bishop, Color.White);
        squares[7, 6] = new Piece(PieceType.Knight, Color.White);
        squares[7, 7] = new Piece(PieceType.Rook, Color.White);

        for (int col = 0; col < 8; col++)
            squares[6, col] = new Piece(PieceType.Pawn, Color.White);

        // Peças pretas
        squares[0, 0] = new Piece(PieceType.Rook, Color.Black);
        squares[0, 1] = new Piece(PieceType.Knight, Color.Black);
        squares[0, 2] = new Piece(PieceType.Bishop, Color.Black);
        squares[0, 3] = new Piece(PieceType.Queen, Color.Black);
        squares[0, 4] = new Piece(PieceType.King, Color.Black);
        squares[0, 5] = new Piece(PieceType.Bishop, Color.Black);
        squares[0, 6] = new Piece(PieceType.Knight, Color.Black);
        squares[0, 7] = new Piece(PieceType.Rook, Color.Black);

        for (int col = 0; col < 8; col++)
            squares[1, col] = new Piece(PieceType.Pawn, Color.Black);

        SideToMove = Color.White;
        HalfMoveClock = 0;
        FullMoveNumber = 1;
        CastlingRights = new[] { true, true, true, true }; // Todos podem fazer roque inicialmente
        EnPassantSquare = null;
    }

    public Piece GetPiece(Position pos)
    {
        if (!pos.IsValid) return new Piece(PieceType.None, Color.White);
        return squares[pos.Row, pos.Col];
    }

    public void SetPiece(Position pos, Piece piece)
    {
        if (pos.IsValid)
            squares[pos.Row, pos.Col] = piece;
    }

    public void MakeMove(Move move)
    {
        Piece movingPiece = GetPiece(move.From);
        Piece capturedPiece = GetPiece(move.To);

        // Atualizar contador de meio-movimentos
        if (movingPiece.Type == PieceType.Pawn || !capturedPiece.IsEmpty)
            HalfMoveClock = 0;
        else
            HalfMoveClock++;

        // Limpar en passant anterior
        EnPassantSquare = null;

        // Verificar se é avanço duplo de peão (para en passant)
        if (movingPiece.Type == PieceType.Pawn)
        {
            int rowDiff = Math.Abs(move.To.Row - move.From.Row);
            if (rowDiff == 2)
            {
                EnPassantSquare = new Position(
                    (move.From.Row + move.To.Row) / 2,
                    move.From.Col
                );
            }

            // Captura en passant
            if (move.To.Col != move.From.Col && capturedPiece.IsEmpty)
            {
                // Remove o peão capturado en passant
                int capturedRow = SideToMove == Color.White ? move.To.Row + 1 : move.To.Row - 1;
                SetPiece(new Position(capturedRow, move.To.Col), new Piece(PieceType.None, Color.White));
            }

            // Promoção
            if (move.PromotionPiece != PieceType.None)
            {
                movingPiece = new Piece(move.PromotionPiece, movingPiece.Color);
            }
        }

        // Atualizar direitos de roque
        if (movingPiece.Type == PieceType.King)
        {
            if (SideToMove == Color.White)
            {
                CastlingRights[0] = false; // WK
                CastlingRights[1] = false; // WQ
            }
            else
            {
                CastlingRights[2] = false; // BK
                CastlingRights[3] = false; // BQ
            }

            // Verificar se é roque e mover a torre
            if (Math.Abs(move.To.Col - move.From.Col) == 2)
            {
                if (move.To.Col == 6) // Roque curto
                {
                    Position rookFrom = new Position(move.From.Row, 7);
                    Position rookTo = new Position(move.From.Row, 5);
                    Piece rook = GetPiece(rookFrom);
                    SetPiece(rookTo, rook);
                    SetPiece(rookFrom, new Piece(PieceType.None, Color.White));
                }
                else if (move.To.Col == 2) // Roque longo
                {
                    Position rookFrom = new Position(move.From.Row, 0);
                    Position rookTo = new Position(move.From.Row, 3);
                    Piece rook = GetPiece(rookFrom);
                    SetPiece(rookTo, rook);
                    SetPiece(rookFrom, new Piece(PieceType.None, Color.White));
                }
            }
        }

        // Atualizar direitos de roque se uma torre se move
        if (movingPiece.Type == PieceType.Rook)
        {
            if (SideToMove == Color.White)
            {
                if (move.From.Row == 7 && move.From.Col == 7) CastlingRights[0] = false;
                if (move.From.Row == 7 && move.From.Col == 0) CastlingRights[1] = false;
            }
            else
            {
                if (move.From.Row == 0 && move.From.Col == 7) CastlingRights[2] = false;
                if (move.From.Row == 0 && move.From.Col == 0) CastlingRights[3] = false;
            }
        }

        // Fazer o movimento
        SetPiece(move.To, movingPiece);
        SetPiece(move.From, new Piece(PieceType.None, Color.White));

        // Trocar o lado que joga
        if (SideToMove == Color.Black)
            FullMoveNumber++;
        SideToMove = SideToMove == Color.White ? Color.Black : Color.White;
    }

    public bool IsSquareAttacked(Position pos, Color byColor)
    {
        // Verifica se uma casa está sendo atacada por uma cor específica
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Position from = new Position(row, col);
                Piece piece = GetPiece(from);

                if (piece.IsEmpty || piece.Color != byColor)
                    continue;

                if (CanPieceAttack(from, pos, piece))
                    return true;
            }
        }
        return false;
    }

    private bool CanPieceAttack(Position from, Position to, Piece piece)
    {
        int rowDiff = to.Row - from.Row;
        int colDiff = to.Col - from.Col;
        int absRowDiff = Math.Abs(rowDiff);
        int absColDiff = Math.Abs(colDiff);

        switch (piece.Type)
        {
            case PieceType.Pawn:
                int direction = piece.Color == Color.White ? -1 : 1;
                return rowDiff == direction && absColDiff == 1;

            case PieceType.Knight:
                return (absRowDiff == 2 && absColDiff == 1) || (absRowDiff == 1 && absColDiff == 2);

            case PieceType.Bishop:
                if (absRowDiff != absColDiff) return false;
                return IsPathClear(from, to);

            case PieceType.Rook:
                if (rowDiff != 0 && colDiff != 0) return false;
                return IsPathClear(from, to);

            case PieceType.Queen:
                if (rowDiff != 0 && colDiff != 0 && absRowDiff != absColDiff) return false;
                return IsPathClear(from, to);

            case PieceType.King:
                return absRowDiff <= 1 && absColDiff <= 1;

            default:
                return false;
        }
    }

    private bool IsPathClear(Position from, Position to)
    {
        int rowStep = Math.Sign(to.Row - from.Row);
        int colStep = Math.Sign(to.Col - from.Col);

        int currentRow = from.Row + rowStep;
        int currentCol = from.Col + colStep;

        while (currentRow != to.Row || currentCol != to.Col)
        {
            if (!GetPiece(new Position(currentRow, currentCol)).IsEmpty)
                return false;

            currentRow += rowStep;
            currentCol += colStep;
        }

        return true;
    }

    public bool IsInCheck(Color color)
    {
        // Encontrar o rei
        Position? kingPos = null;
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Position pos = new Position(row, col);
                Piece piece = GetPiece(pos);
                if (piece.Type == PieceType.King && piece.Color == color)
                {
                    kingPos = pos;
                    break;
                }
            }
            if (kingPos.HasValue) break;
        }

        if (!kingPos.HasValue) return false;

        return IsSquareAttacked(kingPos.Value, color == Color.White ? Color.Black : Color.White);
    }

    public void Display()
    {
        Console.WriteLine("\n  a b c d e f g h");
        Console.WriteLine(" +-----------------+");
        for (int row = 0; row < 8; row++)
        {
            Console.Write($"{8 - row}| ");
            for (int col = 0; col < 8; col++)
            {
                Console.Write($"{GetPiece(new Position(row, col))} ");
            }
            Console.WriteLine($"|{8 - row}");
        }
        Console.WriteLine(" +-----------------+");
        Console.WriteLine("  a b c d e f g h\n");
        Console.WriteLine($"Vez de: {(SideToMove == Color.White ? "Brancas" : "Pretas")}");
    }
}