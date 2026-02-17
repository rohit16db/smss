import React from 'react';

interface FormErrorProps {
  error?: string;
  'aria-live'?: 'polite' | 'assertive' | 'off';
}

export const FormError: React.FC<FormErrorProps> = ({ error, 'aria-live': ariaLive = 'polite' }) => {
  if (!error) return null;

  return (
    <div 
      className="mt-1 text-sm text-red-600" 
      role="alert"
      aria-live={ariaLive}
    >
      {error}
    </div>
  );
};

interface InputFieldProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  required?: boolean;
  helpText?: string;
}

export const InputField: React.FC<InputFieldProps> = ({
  label,
  error,
  required,
  helpText,
  id,
  ...props
}) => {
  const inputId = id || `input-${label.toLowerCase().replace(/\s+/g, '-')}`;
  const errorId = `${inputId}-error`;
  const helpId = `${inputId}-help`;

  return (
    <div>
      <label 
        htmlFor={inputId} 
        className="block text-sm font-medium text-gray-700 mb-1"
      >
        {label}
        {required && <span className="text-red-500 ml-1" aria-label="required">*</span>}
      </label>
      <input
        id={inputId}
        aria-invalid={error ? 'true' : 'false'}
        aria-describedby={`${error ? errorId : ''} ${helpText ? helpId : ''}`.trim()}
        className={`input-field ${error ? 'border-red-500 focus:ring-red-500 focus:border-red-500' : ''}`}
        {...props}
      />
      {helpText && !error && (
        <p id={helpId} className="mt-1 text-sm text-gray-500">
          {helpText}
        </p>
      )}
      {error && <FormError error={error} />}
    </div>
  );
};

interface TextAreaFieldProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  label: string;
  error?: string;
  required?: boolean;
  helpText?: string;
}

export const TextAreaField: React.FC<TextAreaFieldProps> = ({
  label,
  error,
  required,
  helpText,
  id,
  ...props
}) => {
  const inputId = id || `textarea-${label.toLowerCase().replace(/\s+/g, '-')}`;
  const errorId = `${inputId}-error`;
  const helpId = `${inputId}-help`;

  return (
    <div>
      <label 
        htmlFor={inputId} 
        className="block text-sm font-medium text-gray-700 mb-1"
      >
        {label}
        {required && <span className="text-red-500 ml-1" aria-label="required">*</span>}
      </label>
      <textarea
        id={inputId}
        aria-invalid={error ? 'true' : 'false'}
        aria-describedby={`${error ? errorId : ''} ${helpText ? helpId : ''}`.trim()}
        className={`input-field resize-none ${error ? 'border-red-500 focus:ring-red-500 focus:border-red-500' : ''}`}
        {...props}
      />
      {helpText && !error && (
        <p id={helpId} className="mt-1 text-sm text-gray-500">
          {helpText}
        </p>
      )}
      {error && <FormError error={error} />}
    </div>
  );
};
