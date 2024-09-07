import {Vector} from "three/examples/jsm/physics/RapierPhysics";

export interface SectorInstanceItem {
    id: string;
    sector: Vector;
    expiresAt: string;
    forceExpiresAt: string;
    createdAt: string;
    onLoadScript: string;
    onSectorEnterScript: string;
}

const baseUrl = process.env.REACT_APP_BACKEND_URL;

const getAll = async (): Promise<SectorInstanceItem[]> => {

    const response = await fetch(`${baseUrl}/sector/instance`);

    return response.json();
}

const forceExpireAll = async (): Promise<void> => {
    const response = await fetch(`${baseUrl}/sector/instance/expire/force/all`, {
        method: 'POST',
    });

    return response.json();
}

export {
    getAll,
    forceExpireAll
}