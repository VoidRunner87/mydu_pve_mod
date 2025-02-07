
export interface SectorEncounterItem {
    id: string;
    name: string;
    onLoadScript: string;
    onSectorEnterScript: string;
    active: string;
    properties: SectorEncounterProperties;
}

export interface SectorEncounterProperties {
    tags: string[];
}

const getAll = async (): Promise<SectorEncounterItem[]> => {

    const baseUrl = process.env.REACT_APP_BACKEND_URL;

    const response = await fetch(`${baseUrl}/sector/encounter`);

    return response.json();
}

export {
    getAll
}