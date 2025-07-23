import * as yup from "yup";

export function yupErrorsToObject(errors: yup.ValidationError[]): Record<string, string> {
    const map = Object.create(null);
    errors.forEach(error => {
        if (error.path) {
            map[error.path] = error.message;
        }
    });
    return map;
}