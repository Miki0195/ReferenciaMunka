export interface ProblemDetails {
    type: string;
    title: string;
    status: number;
    traceId: string;
    detail?: string;
    errors?: Record<string, string[]>;
}