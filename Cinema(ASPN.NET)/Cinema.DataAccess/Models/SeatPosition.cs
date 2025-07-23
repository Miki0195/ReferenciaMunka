using Microsoft.EntityFrameworkCore;

namespace ELTE.Cinema.DataAccess.Models;

[Owned]
public record SeatPosition(int Row, int Column);