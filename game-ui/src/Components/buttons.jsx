import styled from "styled-components";
import {TargetIcon2} from "./icons";

export const IconButton = styled.button`
    background-color: rgb(250, 212, 122);
    padding: 2px 2px 0;
    font-weight: bold;
    text-transform: uppercase;
    color: black;
    text-align: left;
    border: none;
    cursor: pointer;
`;

export const PrimaryButton = styled.button`
    background-color: rgb(250, 212, 122);
    padding: 8px;
    font-weight: bold;
    text-transform: uppercase;
    color: black;
    text-align: left;
    border: none;
    cursor: pointer;
`;

export const DestructiveButton = styled.button`
    background-color: rgb(250, 80, 80);
    padding: 8px;
    font-weight: bold;
    text-transform: uppercase;
    color: white;
    text-align: left;
    border: none;
    cursor: pointer;
`;

export const SecondaryButton = styled.button`
    background-color: rgb(27, 48, 56);
    color: rgb(180, 221, 235);
    padding: 8px;
    font-weight: bold;
    text-transform: uppercase;
    text-align: left;
    border: none;
    cursor: pointer;
`;

export const TargetButton = ({onClick}) => {
    return (
        <IconButton onClick={onClick}><TargetIcon2 /></IconButton>
    );
}