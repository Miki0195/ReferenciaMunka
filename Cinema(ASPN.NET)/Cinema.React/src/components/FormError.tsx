interface Props {
    message?: string;
}

export function FormError({ message }: Props) {
    return message?.length !== 0 ? <p className="text-danger">{message}</p> : null;
}