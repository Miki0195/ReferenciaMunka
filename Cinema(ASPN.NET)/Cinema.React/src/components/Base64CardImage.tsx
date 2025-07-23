import Card from 'react-bootstrap/Card';

interface Props {
    data: string;
    alt: string;
    variant: "top" | "bottom";
}

export function Base64CardImage({ data, alt, variant }: Props) {
    return <Card.Img variant={variant} src={`data:image/jpg;base64, ${data}`} alt={alt} />;
}