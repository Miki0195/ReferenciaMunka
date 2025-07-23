import { HttpError } from "@/api/errors/HttpError";

export class ServerSideValidationError extends HttpError {
    public readonly validationErrors: Record<string, string>;

    constructor(status: number, message: string, validationErrors: Record<string, string>) {
        super(status, message);
        this.validationErrors = validationErrors;
    }
}