import styled from "styled-components";

export const Widget = styled.div`
    font-family: Play, Arial, Helvetica, sans-serif;
    border-top: 4px solid #B6DFED;
    border-bottom: 1px solid #B6DFED;
    color: white;
    min-width: 350px;
    max-width: 400px;
    background-color: rgba(13, 24, 28, 0.4);
    position: absolute;
`;

export const WidgetHeader = styled.div`
    background: linear-gradient(to right, #21333A, #304953 50%, #21333A);
    text-align: center;
    text-transform: uppercase;
    padding: 8px;
    display: flex;
`;

export const WidgetContainer = styled.div`
    color: #ADD4E1;
    text-transform: uppercase;
    font-size: 0.75em;
    padding-bottom: 1px;
`;

export const WidgetRow = styled.div`
    margin: 4px;
    padding: 8px;
    cursor: pointer;
    background-color: rgba(13, 24, 28, 1);
    border-radius: 2px;
    
    &:hover {
        background-color: rgba(13, 24, 28, 0.5);
    }
`;

export const WidgetButtonRow = styled.div`
    margin: 4px;
    padding: 8px;
    cursor: pointer;
    background-color: rgba(13, 24, 28, 1);
    border-radius: 2px;
    display: flex;
`;

export const PlayerName = styled.div`
    display: flex;
    flex-flow: row;
    flex-grow: 1;
`;

export const Role = styled.div`
    display: flex;
    align-content: end;
`;

export const GridRow = styled.div`
    display: flex;
    margin-bottom: 8px;

    &:last-child {
        margin-bottom: 0;
    }
`;

export const GridRowBars = styled.div`
    display: flex;
    margin-bottom: 3px;

    &:last-child {
        margin-bottom: 0;
    }
`;


export const ConstructName = styled.div`
    display: flex;
    flex-flow: row;
    flex-grow: 1;
    text-overflow: fade;
    white-space: nowrap;
    overflow: hidden;
    margin-right: 8px;
`;

export const ConstructSize = styled.div`
    display: flex;
    font-weight: bold;
    margin-right: 4px;
`;

export const PercentageBar = styled.div`
    width: 100%;
    height: 4px;
    background-color: rgba(23, 34, 38, 1);
    border-radius: 1px;
    overflow: hidden;
    position: relative;
`;

export const FilledBar = styled.div`
    height: 100%;
    width: ${({percentage}) => `${percentage}%`}; /* Set width based on percentage prop */
    background-color: ${({color}) => `${color}`};
    transition: width 0.4s ease;
    border-radius: 1px 0 0 1px;
    background-image: repeating-linear-gradient(
            -90deg,
            rgba(13, 24, 28, 0.9) 0,
            rgba(13, 24, 28, 0.9) 1px,
            transparent 1px,
            transparent 15px
    );
    background-size: 15px 100%;
`;

export const GridColShipName = styled.div`
    display: flex;
    width: 50%;
    align-items: center;
`;

export const GridColBars = styled.div`
    width: 50%;
`;

export const WidgetPage = ({visible, children}) => {
    if (!visible)
    {
        return null;
    }

    return (
        <WidgetContainer>{children}</WidgetContainer>
    );
};