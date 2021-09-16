export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE' | 'OPTIONS' | 'HEAD';
export type HttpHeaders = { [key: string]: string | null; };
export type HttpQuery = Partial<{ [key: string]: string | number | null | boolean | Array<string | number | null | boolean> }>; // partial is needed for strict mode

export interface RequestOpts {
    path: string;
    method: HttpMethod;
    headers?: HttpHeaders;
    query?: HttpQuery;
    body?: unknown | FormData;
    responseType?: 'json' | 'blob' | 'arraybuffer' | 'text';
}

export type ResponseArgs = {
    status: number;
    xhr: XMLHttpRequest;
    response: unknown;
};
