
export interface ScriptItem {
    name: string;
    type: string;
    prefab: string;
}

const getAll = async (): Promise<ScriptItem[]> => {

    const baseUrl = process.env.REACT_APP_BACKEND_URL;

    const response = await fetch(`${baseUrl}/script`);

    return response.json();
}

export {
    getAll
}