const BASE_URL = "http://localhost:5258/api";

export async function getApi<T>(path: string): Promise<T> {
    const response = await fetch(`${BASE_URL}/${path}`);

    if (!response.ok) {
        throw new Error(response.statusText);
    }

    return await response.json() as T;
}

export async function postApi<T>(path: string, body: T): Promise<any> {
    const response = await fetch(`${BASE_URL}/${path}`, {
        method: "POST",
        body: JSON.stringify(body)
    });

    if (!response.ok) {
        throw new Error(response.statusText);
    }
}