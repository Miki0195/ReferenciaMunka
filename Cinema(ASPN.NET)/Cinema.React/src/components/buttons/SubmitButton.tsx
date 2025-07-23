import Button from "react-bootstrap/Button";

interface Props {
    text?: string;
    loading?: boolean;
}

/**
 * A Bootstrap button for submitting forms
 * @constructor
 */
export function SubmitButton({ text = "Submit", loading = false }: Props) {
    return <Button type="submit" disabled={loading}>{loading ? "Loading..." : text}</Button>;
}