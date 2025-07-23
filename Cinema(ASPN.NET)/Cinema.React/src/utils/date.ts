export function stringToLocaleDateTime(dateStr: string) {
    return new Date(dateStr).toLocaleString();
}