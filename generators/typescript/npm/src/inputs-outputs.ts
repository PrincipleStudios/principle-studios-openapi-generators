
export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE' | 'OPTIONS' | 'HEAD';
export type HttpHeaders = { [key: string]: string | null; };

/** Standardized request to be sent to the adapter layer */
export type AdapterRequestArgs = {
    path: string;
    method: HttpMethod;
    headers?: HttpHeaders;
    body?: unknown;
    responseType?: 'json' | 'blob' | 'arraybuffer' | 'text';
}

/** Standardize response to be received from the adapter layer */
export type AdapterResponseArgs = {
    status: number;
    getResponseHeader(header: string): string | null;
    response: unknown;
};
