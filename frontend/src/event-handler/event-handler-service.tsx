
export interface EventHandlerItem {
    name: string;
    type: string;
    prefab: string;
}

const getAll = async (): Promise<EventHandlerItem[]> => {

    const baseUrl = process.env.REACT_APP_BACKEND_URL;

    const response = await fetch(`${baseUrl}/event/handler`);

    return response.json();
}

export {
    getAll
}