import Image from 'react-bootstrap/Image';

interface Props {
    data: string;
    alt: string;
    className?: string;
}

export function Base64Image({ data, alt, className = undefined }: Props) {
    return <Image className={className} src={`data:image/jpg;base64, ${data}`} alt={alt} />;
}