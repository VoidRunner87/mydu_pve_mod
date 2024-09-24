import styled from 'styled-components'

export const Container = styled.div`
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    z-index: 99999999 !important;
`;

export const Panel = styled.div`
    width: 55vw;
    height: 75vh;
    background-color: rgb(25, 34, 41);
    border: 1px solid rgb(50, 79, 77);
    padding: 2px;
    justify-content: center;
    align-items: center;
    z-index: 99999999 !important;
`;

export const Header = styled.div`
    background-color: rgb(0, 0, 0);
    padding: 16px;
    display: flex;
    justify-content: space-between;
    align-items: center;
`;

export const Title = styled.span`
    color: rgb(255, 255, 255);
    font-size: 18px;
    margin-right: 10px;
`;

export const PanelCloseButton = styled.button`
    margin-left: auto;
    background-color: transparent;
    border: none;
    font-size: 40px;
    line-height: 0;
    cursor: pointer;
    padding: 0;
    color: rgb(255, 255, 255);
`;

export const CloseButton = () => {
    return (
        <PanelCloseButton>
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="24" height="24">
                <line x1="4" y1="4" x2="20" y2="20" stroke="white" stroke-width="2" stroke-linecap="round"/>
                <line x1="4" y1="20" x2="20" y2="4" stroke="white" stroke-width="2" stroke-linecap="round"/>
            </svg>
        </PanelCloseButton>
    );
}