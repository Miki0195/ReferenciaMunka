import { ReactNode } from "react";
import Table from 'react-bootstrap/Table';
import { SeatRequestDto } from "@/api/models/SeatRequestDto";
import { SeatMapNumberRow } from "@/components/screenings/SeatMapNumberRow";

export interface SeatMapPosition{
    row: number;
    column: number;
}

interface Props {
    rows: number;
    columns: number;
    takenSeats?: SeatMapPosition[];
    selectedSeats: SeatMapPosition[];
    onSelectionChange?: (seat: SeatRequestDto) => void;
}

export function SeatMap({rows, columns, selectedSeats, takenSeats = [],onSelectionChange}: Props) {
    // Build the seatmap
    const seatRows: ReactNode[] = [];

    for (let i = 1; i <= rows; i++) {
        const seatsCells: ReactNode[] = [];
        for (let j = 1; j <= columns; j++) {
            let cellClass = "table-light";
            let status = "Free";
            if (takenSeats.some(s => s.row === i && s.column === j)) {
                cellClass = "table-warning";
                status = "Unavailable";
            } else if (selectedSeats.some(s => s.row === i && s.column === j)) {
                cellClass = "table-success";
                status = "Selected";
            }
            const clickable = onSelectionChange && status !== "Unavailable";

            function handleClick() {
                if (clickable) {
                    onSelectionChange({row: i, column: j});
                }
            }

            const seat = (
                <td
                    key={`seat-${i}-${j}`}
                    className={`${cellClass} text-center`}
                    title={`Row: ${i} Column: ${j} Status: ${status}`}
                    role={clickable ? "button" : "none"}
                    onClick={handleClick}
                />
            );
            seatsCells.push(seat);

            // Add an empty column for the aisle
            if (j === columns / 2) {
                const placeholder = <td key={`middle-placeholder-seat-${i}`}/>
                seatsCells.push(placeholder);
            }
        }

        // Combine the seats and the numbers on both sides
        const row = (
            <tr key={`row-${i}}`}>
                <td className="text-start">{i}</td>
                {seatsCells}
                <td className="text-end">{i}</td>
            </tr>
        );
        seatRows.push(row);
    }

    return (
        <Table bordered variant="secondary" className="table-layout-fixed fw-bold">
            <thead>
                <SeatMapNumberRow columns={columns}/>
            </thead>
            <tbody>
                {seatRows}
            </tbody>
            <tfoot>
                <SeatMapNumberRow columns={columns}/>
            </tfoot>
        </Table>
    )
}