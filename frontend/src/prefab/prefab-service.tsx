
export interface PrefabItem {
    id: string;
    name: string;
    folder: string;
    path: string;
}

const getAll = async (): Promise<PrefabItem[]> => {

    const baseUrl = process.env.REACT_APP_BACKEND_URL;

    const response = await fetch(`${baseUrl}/prefab`);

    return response.json();
}

export {
    getAll
}