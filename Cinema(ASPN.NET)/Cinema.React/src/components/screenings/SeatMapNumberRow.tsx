import { ReactNode } from "react";

interface Props {
    columns: number;   
}

export function SeatMapNumberRow({ columns }: Props) {
    const columnNumbers: ReactNode[] = [];
    columnNumbers.push(<td key="left-palceholder" />);
    for (let i = 1; i <= columns; i++) {
        columnNumbers.push(<td key={`column-${i}`}>{i}</td>)

        // Add an empty column for the aisle
        if (i === columns / 2) {
            const placeholder = <td key={`middle-placeholder-number-${i}`} />
            columnNumbers.push(placeholder);
        }
    }
    columnNumbers.push(<td key="right-palceholder" />);

    return <tr className="text-center">{columnNumbers}</tr>;
}
