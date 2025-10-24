namespace ChessEduca;

// Enums para representar as peças e cores
public enum PieceType
{
    None = 0,
    Pawn = 1,
    Knight = 2,
    Bishop = 3,
    Rook = 4,
    Queen = 5,
    King = 6
}

public enum Color
{
    White = 0,
    Black = 1
}

// Estrutura para representar uma peça
public struct Piece
{
    public PieceType Type { get; set; }
    public Color Color { get; set; }

    public Piece(PieceType type, Color color)
    {
        Type = type;
        Color = color;
    }

    public bool IsEmpty => Type == PieceType.None;

    public override string ToString()
    {
        if (IsEmpty) return ".";

        char symbol = Type switch
        {
            PieceType.Pawn => 'P',
            PieceType.Knight => 'N',
            PieceType.Bishop => 'B',
            PieceType.Rook => 'R',
            PieceType.Queen => 'Q',
            PieceType.King => 'K',
            _ => '.'
        };

        return Color == Color.White ? symbol.ToString() : symbol.ToString().ToLower();
    }
}

// Estrutura para representar uma posição no tabuleiro
public struct Position
{
    public int Row { get; set; }
    public int Col { get; set; }

    public Position(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public bool IsValid => Row >= 0 && Row < 8 && Col >= 0 && Col < 8;

    public override string ToString() => $"{(char)('a' + Col)}{8 - Row}";
}

// Estrutura para representar um movimento
public struct Move
{
    public Position From { get; set; }
    public Position To { get; set; }
    public PieceType PromotionPiece { get; set; }
    public int Score { get; set; } // Para ordenação de movimentos

    public Move(Position from, Position to, PieceType promotion = PieceType.None)
    {
        From = from;
        To = to;
        PromotionPiece = promotion;
        Score = 0;
    }

    public override string ToString() => $"{From}{To}";
}