import Form from 'react-bootstrap/Form';
import { LoginRequestDto, loginRequestValidator } from "@/api/models/LoginRequestDto";
import { SubmitButton } from "@/components/buttons/SubmitButton";
import { FormError } from "@/components/FormError";
import { useState } from "react";
import { useUserContext } from "@/contexts/UserContext";
import { yupErrorsToObject } from "@/utils/forms";
import * as yup from "yup";
import { ErrorAlert } from "@/components/alerts/ErrorAlert";
import { useLocation, useNavigate } from "react-router-dom";
import { ChangeEvent, FormEvent } from "react";
import { ServerSideValidationError } from "@/api/errors/ServerSideValidationError";
import { HttpError } from "@/api/errors/HttpError";

export function LoginPage() {
    const userContext = useUserContext();
    const navigate = useNavigate();
    const location = useLocation();
    const [loading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [loginData, setLoginData] = useState<LoginRequestDto>({
        email: "",
        password: "",
    });
    const [formErrors, setFormErrors] = useState<Record<string, string>>({});

    function handleInputChange(e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) {
        setLoginData(prevState => ({
            ...prevState,
            [e.target.name]: e.target.value,
        }));
    }

    async function handleFormSubmit(evt: FormEvent<HTMLFormElement>) {
        evt.preventDefault();
        
        setError(null);
        setFormErrors({});
        setIsLoading(true);
        
        try {
           await loginRequestValidator.validate(loginData, { abortEarly: false });
           await userContext.handleLogin(loginData);
           if (location.state?.loginRedirect) {
                navigate(location.state.loginRedirect);
           } else {
               navigate("/");
           }
        } catch (e) {
            if (e instanceof yup.ValidationError) {
                setFormErrors(yupErrorsToObject(e.inner));
            } else if (e instanceof ServerSideValidationError) {
                setFormErrors(e.validationErrors);
            } else if (e instanceof HttpError) {
                setError(e.message);
            } else {
                setError("Unknown error")
            }
        } finally {
            setIsLoading(false);
        }
    }
    
    return (
        <>
            {error ? <ErrorAlert message={error} /> : null}
            <h1>Login</h1>
            {/* Disable default HTML validation, because we use Yup */}
            <Form onSubmit={handleFormSubmit} validated={false}>
                <Form.Group className="mb-3">
                    <Form.Label htmlFor="email">Email:</Form.Label>
                    <Form.Control
                        type="text"
                        className="form-control"
                        id="email"
                        name="email"
                        onChange={handleInputChange}
                        value={loginData.email}
                    />
                    <FormError message={formErrors.email}/>
                </Form.Group>

                <Form.Group className="mb-3">
                    <Form.Label htmlFor="password">Password:</Form.Label>
                    <Form.Control
                        type="password"
                        className="form-control"
                        id="password"
                        name="password"
                        onChange={handleInputChange}
                        value={loginData.password}
                    />
                    <FormError message={formErrors.password}/>
                </Form.Group>
                <SubmitButton text="Login" loading={loading}/>
            </Form>
        </>
    );
}